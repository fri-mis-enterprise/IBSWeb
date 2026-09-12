using System.Globalization;
using System.Linq.Expressions;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.DTOs;
using IBS.Models.Enums;
using IBS.Models.Filpride.AccountsReceivable;
using IBS.Models.Filpride.Books;
using IBS.Models.Filpride.MasterFile;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Repository.Filpride
{
    public class ProvisionalReceiptRepository : Repository<FilprideProvisionalReceipt>, IProvisionalReceiptRepository
    {
        private readonly ApplicationDbContext _db;

        public ProvisionalReceiptRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<string> GenerateSeriesNumberAsync(string company, string type, CancellationToken cancellationToken = default)
        {
            return type switch
            {
                nameof(DocumentType.Documented) => await GenerateCodeForDocumented(company, cancellationToken),
                nameof(DocumentType.Undocumented) => await GenerateCodeForUnDocumented(company, cancellationToken),
                _ => throw new ArgumentException("Invalid type")
            };
        }

        public async Task PostAsync(int receiptId, string postedBy, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var receipt = await LockReceiptAsync(receiptId, cancellationToken);
                if (receipt.Status != nameof(CollectionReceiptStatus.Pending) || receipt.PostedBy != null ||
                    receipt.PostedDate != null || receipt.CanceledBy != null || receipt.VoidedBy != null)
                {
                    throw new InvalidOperationException("Only pending provisional receipts can be posted.");
                }

                if (receipt.CheckDate.HasValue && receipt.CheckDate.Value > DateTimeHelper.GetLastDayOfMonth())
                {
                    throw new InvalidOperationException("Future-dated checks cannot be posted.");
                }

                if (string.IsNullOrWhiteSpace(postedBy))
                {
                    throw new InvalidOperationException("The posting user could not be identified.");
                }

                var paymentAmount = receipt.CashAmount + receipt.CheckAmount + receipt.ManagersCheckAmount;
                var fullTotal = paymentAmount + receipt.EWT + receipt.WVAT;
                if (receipt.CashAmount < 0 || receipt.CheckAmount < 0 || receipt.ManagersCheckAmount < 0 ||
                    receipt.EWT < 0 || receipt.WVAT < 0 || fullTotal <= 0 || receipt.Total != fullTotal)
                {
                    throw new InvalidOperationException("Receipt amounts must be non-negative, total must be positive, and the saved total must match its payment and withholding amounts.");
                }

                var category = await _db.FilprideCollectionCategories
                    .IgnoreQueryFilters()
                    .Include(c => c.CreditAccount)
                    .SingleOrDefaultAsync(c => c.Id == receipt.CollectionCategoryId, cancellationToken)
                    ?? throw new InvalidOperationException("The receipt collection category could not be found.");
                if (!Enum.IsDefined(category.TaggingRequirement))
                {
                    throw new InvalidOperationException("The receipt category has an invalid tagging requirement.");
                }
                var creditAccount = category.CreditAccount;
                if (creditAccount.HasChildren ||
                    string.IsNullOrWhiteSpace(creditAccount.AccountNumber) ||
                    string.IsNullOrWhiteSpace(creditAccount.AccountName))
                {
                    throw new InvalidOperationException("The category must use a valid credit account with no child accounts.");
                }

                var subAccount = await ResolveCategorySubAccountAsync(receipt, category, cancellationToken);
                var accountTitles = await GetListOfAccountTitleDto(cancellationToken);
                var cashInBank = accountTitles.SingleOrDefault(a => a.AccountNumber == "101010100")
                                 ?? throw new InvalidOperationException("Account title '101010100' not found.");
                var cwt = accountTitles.SingleOrDefault(a => a.AccountNumber == "101060400")
                          ?? throw new InvalidOperationException("Account title '101060400' not found.");
                var cwv = accountTitles.SingleOrDefault(a => a.AccountNumber == "101060600")
                          ?? throw new InvalidOperationException("Account title '101060600' not found.");

                var postedDateAndTime = DateTimeHelper.GetCurrentPhilippineTime();
                var postedDate = DateOnly.FromDateTime(postedDateAndTime);
                var description = $"Collection of Provisional Receipt# {receipt.SeriesNumber} from {receipt.PayerName}";
                var ledgers = new List<FilprideGeneralLedgerBook>();

                void AddDebit(AccountTitleDto account, decimal amount)
                {
                    if (amount <= 0)
                    {
                        return;
                    }

                    ledgers.Add(new FilprideGeneralLedgerBook
                    {
                        Date = postedDate,
                        Reference = receipt.SeriesNumber,
                        Description = description,
                        AccountId = account.AccountId,
                        AccountNo = account.AccountNumber,
                        AccountTitle = account.AccountName,
                        Debit = amount,
                        Credit = 0,
                        CreatedBy = postedBy,
                        CreatedDate = postedDateAndTime,
                        ModuleType = nameof(ModuleType.Collection)
                    });
                }

                AddDebit(cashInBank, paymentAmount);
                AddDebit(cwt, receipt.EWT);
                AddDebit(cwv, receipt.WVAT);
                ledgers.Add(new FilprideGeneralLedgerBook
                {
                    Date = postedDate,
                    Reference = receipt.SeriesNumber,
                    Description = description,
                    AccountId = creditAccount.AccountId,
                    AccountNo = creditAccount.AccountNumber!,
                    AccountTitle = creditAccount.AccountName,
                    Debit = 0,
                    Credit = fullTotal,
                    CreatedBy = postedBy,
                    CreatedDate = postedDateAndTime,
                    SubAccountType = subAccount.Type,
                    SubAccountId = subAccount.Id,
                    SubAccountName = subAccount.Name,
                    ModuleType = nameof(ModuleType.Collection)
                });

                receipt.PostedBy = postedBy;
                receipt.PostedDate = postedDateAndTime;
                receipt.Status = nameof(CollectionReceiptStatus.Posted);
                _db.FilprideGeneralLedgerBooks.AddRange(ledgers);
                _db.FilprideAuditTrails.Add(new FilprideAuditTrail(postedBy,
                    $"Posted provisional receipt# {receipt.SeriesNumber}", "Provisional Receipt"));

                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task UnpostAsync(int receiptId, string unpostedBy, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var receipt = await LockReceiptAsync(receiptId, cancellationToken);
                if (receipt.PostedDate == null || receipt.PostedBy == null ||
                    receipt.CanceledBy != null || receipt.VoidedBy != null)
                {
                    throw new InvalidOperationException("The provisional receipt must be posted before it can be unposted.");
                }

                var ledgerEntries = await _db.FilprideGeneralLedgerBooks
                    .Where(entry => entry.Reference == receipt.SeriesNumber &&
                                    entry.ModuleType == nameof(ModuleType.Collection))
                    .ToListAsync(cancellationToken);

                _db.FilprideGeneralLedgerBooks.RemoveRange(ledgerEntries);
                receipt.PostedBy = null;
                receipt.PostedDate = null;
                receipt.Status = nameof(CollectionReceiptStatus.Pending);
                _db.FilprideAuditTrails.Add(new FilprideAuditTrail(unpostedBy,
                    $"Unposted provisional receipt# {receipt.SeriesNumber}", "Provisional Receipt"));

                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<FilprideProvisionalReceipt> LockReceiptAsync(int receiptId, CancellationToken cancellationToken)
        {
            var receipts = await _db.FilprideProvisionalReceipts
                       .FromSqlInterpolated($"SELECT * FROM filpride_provisional_receipts WHERE id = {receiptId} FOR UPDATE")
                       .ToListAsync(cancellationToken);
            return receipts.SingleOrDefault()
                   ?? throw new KeyNotFoundException("Provisional receipt id not found.");
        }

        private async Task<(SubAccountType? Type, int? Id, string? Name)> ResolveCategorySubAccountAsync(
            FilprideProvisionalReceipt receipt,
            FilprideCollectionCategory category,
            CancellationToken cancellationToken)
        {
            if (receipt.TagType is null)
            {
                if (receipt.TaggedCompanyId != null || receipt.TaggedSupplierId != null || receipt.TaggedBankAccountId != null ||
                    category.TaggingRequirement == CollectionTaggingRequirement.Required)
                {
                    throw new InvalidOperationException("The receipt does not have the required valid category tag.");
                }

                return (null, null, null);
            }

            if (!Enum.IsDefined(receipt.TagType.Value) || !category.Allows(receipt.TagType.Value) ||
                category.TaggingRequirement == CollectionTaggingRequirement.None)
            {
                throw new InvalidOperationException("The receipt master-file tag is not allowed by its category.");
            }

            switch (receipt.TagType.Value)
            {
                case CollectionTagType.Company when receipt.TaggedCompanyId is > 0 &&
                                                     receipt.TaggedSupplierId == null && receipt.TaggedBankAccountId == null:
                    var company = await _db.Companies.AsNoTracking()
                        .SingleOrDefaultAsync(c => c.CompanyId == receipt.TaggedCompanyId, cancellationToken)
                        ?? throw new InvalidOperationException("The tagged company could not be found.");
                    return (SubAccountType.Company, company.CompanyId, $"{company.CompanyCode} - {company.CompanyName}");

                case CollectionTagType.Employee when receipt.TaggedSupplierId is > 0 &&
                                                      receipt.TaggedCompanyId == null && receipt.TaggedBankAccountId == null:
                    var employee = await _db.FilprideSuppliers.AsNoTracking()
                        .SingleOrDefaultAsync(s => s.SupplierId == receipt.TaggedSupplierId && s.Category == "Employee", cancellationToken)
                        ?? throw new InvalidOperationException("The tagged employee could not be found.");
                    return (SubAccountType.Supplier, employee.SupplierId, employee.SupplierName);

                case CollectionTagType.BankAccount when receipt.TaggedBankAccountId is > 0 &&
                                                         receipt.TaggedCompanyId == null && receipt.TaggedSupplierId == null:
                    var bankAccount = await _db.FilprideBankAccounts.AsNoTracking()
                        .SingleOrDefaultAsync(b => b.BankAccountId == receipt.TaggedBankAccountId, cancellationToken)
                        ?? throw new InvalidOperationException("The tagged bank account could not be found.");
                    return (SubAccountType.BankAccount, bankAccount.BankAccountId,
                        $"{bankAccount.Bank} - {bankAccount.AccountNo} - {bankAccount.AccountName}");

                default:
                    throw new InvalidOperationException("The receipt does not have a valid master-file tag.");
            }
        }

        private async Task<string> GenerateCodeForDocumented(string company, CancellationToken cancellationToken = default)
        {
            var lastCr = await _db
                .FilprideProvisionalReceipts
                .AsNoTracking()
                .OrderByDescending(x => x.SeriesNumber.Length)
                .ThenByDescending(x => x.SeriesNumber)
                .FirstOrDefaultAsync(x =>

                    x.Type == nameof(DocumentType.Documented),
                    cancellationToken);

            if (lastCr == null)
            {
                return "PR0000000001";
            }

            var lastSeries = lastCr.SeriesNumber;
            var numericPart = lastSeries.Substring(2);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 2) + incrementedNumber.ToString("D10");
        }

        private async Task<string> GenerateCodeForUnDocumented(string company, CancellationToken cancellationToken = default)
        {
            var lastCr = await _db
                .FilprideProvisionalReceipts
                .AsNoTracking()
                .OrderByDescending(x => x.SeriesNumber.Length)
                .ThenByDescending(x => x.SeriesNumber)
                .FirstOrDefaultAsync(x =>

                        x.Type == nameof(DocumentType.Undocumented),
                    cancellationToken);

            if (lastCr == null)
            {
                return "PRU000000001";
            }

            var lastSeries = lastCr.SeriesNumber;
            var numericPart = lastSeries.Substring(3);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 3) + incrementedNumber.ToString("D9");
        }

        public async Task ApplyClearingDateAsync(FilprideProvisionalReceipt provisionalReceipt, CancellationToken cancellationToken = default)
        {
            var ledgers = new List<FilprideGeneralLedgerBook>();
            var accountTitlesDto = await GetListOfAccountTitleDto(cancellationToken);
            var cashInBankTitle = accountTitlesDto.Find(c => c.AccountNumber == "101010100")
                                  ?? throw new ArgumentException("Account title '101010100' not found.");

            var payerName = provisionalReceipt.PayerName;

            var description = $"PR Ref collected from {payerName} Check No. {provisionalReceipt.CheckNo} issued by {provisionalReceipt.BankAccountNo} {provisionalReceipt.BankAccountName}";

            ledgers.Add(
                new FilprideGeneralLedgerBook
                {
                    Date = provisionalReceipt.ClearedDate!.Value,
                    Reference = provisionalReceipt.SeriesNumber,
                    Description = description,
                    AccountId = cashInBankTitle.AccountId,
                    AccountNo = cashInBankTitle.AccountNumber,
                    AccountTitle = cashInBankTitle.AccountName,
                    Debit = provisionalReceipt.CashAmount + provisionalReceipt.CheckAmount + provisionalReceipt.ManagersCheckAmount,
                    Credit = 0,
                    CreatedBy = provisionalReceipt.PostedBy!,
                    CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                    SubAccountType = SubAccountType.BankAccount,
                    SubAccountId = provisionalReceipt.BankId,
                    SubAccountName = provisionalReceipt.BankId.HasValue
                        ? $"{provisionalReceipt.BankAccountNo} {provisionalReceipt.BankAccountName}"
                        : null,
                    ModuleType = nameof(ModuleType.Collection)
                }
            );

            ledgers.Add(
                new FilprideGeneralLedgerBook
                {
                    Date = provisionalReceipt.ClearedDate!.Value,
                    Reference = provisionalReceipt.SeriesNumber,
                    Description = description,
                    AccountId = cashInBankTitle.AccountId,
                    AccountNo = cashInBankTitle.AccountNumber,
                    AccountTitle = cashInBankTitle.AccountName,
                    Debit = 0,
                    Credit = provisionalReceipt.CashAmount + provisionalReceipt.CheckAmount + provisionalReceipt.ManagersCheckAmount,
                    CreatedBy = provisionalReceipt.PostedBy!,
                    CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                    ModuleType = nameof(ModuleType.Collection)
                }
            );

            await _db.FilprideGeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public override async Task<FilprideProvisionalReceipt?> GetAsync(Expression<Func<FilprideProvisionalReceipt, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await dbSet.Where(filter)
                .Include(x => x.CollectionCategory)
                .Include(x => x.TaggedSupplier)
                .Include(x => x.BankAccount)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public override IQueryable<FilprideProvisionalReceipt> GetAllQuery(Expression<Func<FilprideProvisionalReceipt, bool>>? filter = null)
        {
            IQueryable<FilprideProvisionalReceipt> query = dbSet
                .Include(x => x.CollectionCategory)
                .Include(x => x.TaggedSupplier)
                .Include(x => x.BankAccount)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        public override async Task<IEnumerable<FilprideProvisionalReceipt>> GetAllAsync(Expression<Func<FilprideProvisionalReceipt, bool>>? filter, CancellationToken cancellationToken = default)
        {
            IQueryable<FilprideProvisionalReceipt> query = dbSet
                .Include(x => x.CollectionCategory)
                .Include(x => x.TaggedSupplier)
                .Include(x => x.BankAccount);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync(cancellationToken);
        }
    }
}
