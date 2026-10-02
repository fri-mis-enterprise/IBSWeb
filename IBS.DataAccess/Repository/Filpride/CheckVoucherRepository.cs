using System.Linq.Expressions;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.DTOs;
using IBS.Models.Enums;
using IBS.Models.Filpride.AccountsPayable;
using IBS.Models.Filpride.Books;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Repository.Filpride
{
    public class CheckVoucherRepository: Repository<FilprideCheckVoucherHeader>, ICheckVoucherRepository
    {
        private readonly ApplicationDbContext _db;

        public CheckVoucherRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default)
        {
            return type switch
            {
                nameof(DocumentType.Documented) => await GenerateCodeForDocumented(cancellationToken),
                nameof(DocumentType.Undocumented) => await GenerateCodeForUnDocumented(cancellationToken),
                _ => throw new ArgumentException("Invalid type")
            };
        }

        private async Task<string> GenerateCodeForDocumented(CancellationToken cancellationToken = default)
        {
            var lastCv = await _db
                .FilprideCheckVoucherHeaders
                .AsNoTracking()
                .OrderByDescending(x => x.CheckVoucherHeaderNo!.Length)
                .ThenByDescending(x => x.CheckVoucherHeaderNo)
                .FirstOrDefaultAsync(x =>
                    x.Category == "Trade" &&
                    x.Type == nameof(DocumentType.Documented),
                    cancellationToken);

            if (lastCv == null)
            {
                return "CV0000000001";
            }

            var lastSeries = lastCv.CheckVoucherHeaderNo!;
            var numericPart = lastSeries.Substring(2);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 2) + incrementedNumber.ToString("D10");
        }

        private async Task<string> GenerateCodeForUnDocumented(CancellationToken cancellationToken = default)
        {
            var lastCv = await _db
                .FilprideCheckVoucherHeaders
                .AsNoTracking()
                .OrderByDescending(x => x.CheckVoucherHeaderNo!.Length)
                .ThenByDescending(x => x.CheckVoucherHeaderNo)
                .FirstOrDefaultAsync(x =>
                        x.Category == "Trade" &&
                        x.Type == nameof(DocumentType.Undocumented),
                    cancellationToken);

            if (lastCv == null)
            {
                return "CVU000000001";
            }

            var lastSeries = lastCv.CheckVoucherHeaderNo!;
            var numericPart = lastSeries.Substring(3);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 3) + incrementedNumber.ToString("D9");
        }

        public async Task UpdateInvoicingVoucher(decimal paymentAmount, int invoiceVoucherId, CancellationToken cancellationToken = default)
        {
            var invoiceVoucher = await GetAsync(i => i.CheckVoucherHeaderId == invoiceVoucherId, cancellationToken)
                                 ?? throw new InvalidOperationException($"Check voucher with id '{invoiceVoucherId}' not found.");

            var detailsVoucher = await _db.FilprideCheckVoucherDetails
                .Where(cvd => cvd.TransactionNo == invoiceVoucher.CheckVoucherHeaderNo
                              && cvd.AccountNo == "201020200")
                .Select(cvd => cvd.Credit)
                .FirstOrDefaultAsync(cancellationToken);

            invoiceVoucher.AmountPaid += paymentAmount;

            if (invoiceVoucher.AmountPaid >= detailsVoucher)
            {
                invoiceVoucher.IsPaid = true;
                invoiceVoucher.Status = nameof(CheckVoucherInvoiceStatus.Paid);
            }
        }

        public async Task UpdateMultipleInvoicingVoucher(decimal paymentAmount, int invoiceVoucherId, CancellationToken cancellationToken = default)
        {
            var invoiceVoucher = await GetAsync(i => i.CheckVoucherHeaderId == invoiceVoucherId, cancellationToken)
                ?? throw new InvalidOperationException($"Check voucher with id '{invoiceVoucherId}' not found.");

            var detailsVoucher = await _db.FilprideCheckVoucherDetails
                .Where(cvd => invoiceVoucher.CheckVoucherHeaderNo!.Contains(cvd.TransactionNo))
                .Select(cvd => cvd.AmountPaid)
                .SumAsync(cancellationToken);

            invoiceVoucher.AmountPaid += paymentAmount;

            if (invoiceVoucher.Total <= detailsVoucher)
            {
                invoiceVoucher.IsPaid = true;
            }
        }

        public override async Task<FilprideCheckVoucherHeader?> GetAsync(Expression<Func<FilprideCheckVoucherHeader, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await dbSet.Where(filter)
                .Include(cv => cv.BankAccount)
                .Include(cv => cv.Supplier)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public override async Task<IEnumerable<FilprideCheckVoucherHeader>> GetAllAsync(Expression<Func<FilprideCheckVoucherHeader, bool>>? filter, CancellationToken cancellationToken = default)
        {
            IQueryable<FilprideCheckVoucherHeader> query = dbSet
                .Include(cv => cv.BankAccount)
                .Include(cv => cv.Supplier);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public override IQueryable<FilprideCheckVoucherHeader> GetAllQuery(Expression<Func<FilprideCheckVoucherHeader, bool>>? filter = null)
        {
            IQueryable<FilprideCheckVoucherHeader> query = dbSet
                .Include(cv => cv.BankAccount)
                .Include(cv => cv.Supplier)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        public async Task<string> GenerateCodeMultipleInvoiceAsync(string type, CancellationToken cancellationToken = default)
        {
            return type switch
            {
                nameof(DocumentType.Documented) => await GenerateCodeMultipleInvoiceForDocumented(cancellationToken),
                nameof(DocumentType.Undocumented) => await GenerateCodeMultipleInvoiceForUnDocumented(cancellationToken),
                _ => throw new ArgumentException("Invalid type")
            };
        }

        private async Task<string> GenerateCodeMultipleInvoiceForDocumented(CancellationToken cancellationToken = default)
        {
            var lastCv = await _db
                .FilprideCheckVoucherHeaders
                .AsNoTracking()
                .OrderByDescending(x => x.CheckVoucherHeaderNo!.Length)
                .ThenByDescending(x => x.CheckVoucherHeaderNo)
                .FirstOrDefaultAsync(x =>
                        x.CvType == nameof(CVType.Invoicing) &&
                        x.Type == nameof(DocumentType.Documented),
                    cancellationToken);

            if (lastCv == null)
            {
                return "INV000000001";
            }

            var lastSeries = lastCv.CheckVoucherHeaderNo!;
            var numericPart = lastSeries.Substring(3);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 3) + incrementedNumber.ToString("D9");
        }

        private async Task<string> GenerateCodeMultipleInvoiceForUnDocumented(CancellationToken cancellationToken = default)
        {
            var lastCv = await _db
                .FilprideCheckVoucherHeaders
                .AsNoTracking()
                .OrderByDescending(x => x.CheckVoucherHeaderNo!.Length)
                .ThenByDescending(x => x.CheckVoucherHeaderNo)
                .FirstOrDefaultAsync(x =>
                        x.CvType == nameof(CVType.Invoicing) &&
                        x.Type == nameof(DocumentType.Undocumented),
                    cancellationToken);

            if (lastCv == null)
            {
                return "INVU00000001";
            }

            var lastSeries = lastCv.CheckVoucherHeaderNo!;
            var numericPart = lastSeries.Substring(4);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 4) + incrementedNumber.ToString("D8");
        }

        public async Task<string> GenerateCodeMultiplePaymentAsync(string type, CancellationToken cancellationToken = default)
        {
            return type switch
            {
                nameof(DocumentType.Documented) => await GenerateCodeMultiplePaymentForDocumented(cancellationToken),
                nameof(DocumentType.Undocumented) => await GenerateCodeMultiplePaymentForUnDocumented(cancellationToken),
                _ => throw new ArgumentException("Invalid type")
            };
        }

        private async Task<string> GenerateCodeMultiplePaymentForDocumented(CancellationToken cancellationToken = default)
        {
            var lastCv = await _db
                .FilprideCheckVoucherHeaders
                .AsNoTracking()
                .OrderByDescending(x => x.CheckVoucherHeaderNo!.Length)
                .ThenByDescending(x => x.CheckVoucherHeaderNo)
                .FirstOrDefaultAsync(x =>
                        x.CvType == nameof(CVType.Payment) &&
                        x.Type == nameof(DocumentType.Documented),
                    cancellationToken);

            if (lastCv == null)
            {
                return "CVN000000001";
            }

            var lastSeries = lastCv.CheckVoucherHeaderNo!;
            var numericPart = lastSeries.Substring(3);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 3) + incrementedNumber.ToString("D9");
        }

        private async Task<string> GenerateCodeMultiplePaymentForUnDocumented(CancellationToken cancellationToken = default)
        {
            var lastCv = await _db
                .FilprideCheckVoucherHeaders
                .AsNoTracking()
                .OrderByDescending(x => x.CheckVoucherHeaderNo!.Length)
                .ThenByDescending(x => x.CheckVoucherHeaderNo)
                .FirstOrDefaultAsync(x =>
                        x.CvType == nameof(CVType.Payment) &&
                        x.Type == nameof(DocumentType.Undocumented),
                    cancellationToken);

            if (lastCv == null)
            {
                return "CVNU00000001";
            }

            var lastSeries = lastCv.CheckVoucherHeaderNo!;
            var numericPart = lastSeries.Substring(4);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 4) + incrementedNumber.ToString("D8");
        }

        public async Task PostAsync(FilprideCheckVoucherHeader header,
            IEnumerable<FilprideCheckVoucherDetail> details,
            CancellationToken cancellationToken = default)
        {
            if (await ShouldSkipGeneralLedgerEntriesAsync(header, cancellationToken))
            {
                await _db.SaveChangesAsync(cancellationToken);
                return;
            }

            #region --General Ledger Book Recording(CV)--

            var accountTitlesByNumber = (await GetListOfAccountTitleDto(cancellationToken))
                .ToDictionary(account => account.AccountNumber);
            var ledgers = BuildGeneralLedgerEntries(header, details, accountTitlesByNumber);

            await _db.FilprideGeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            #endregion --General Ledger Book Recording(CV)--
        }

        public async Task<int> RebuildGeneralLedgerAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            var checkVouchers = await _db.FilprideCheckVoucherHeaders
                .AsNoTracking()
                .Include(checkVoucher => checkVoucher.Details)
                .Include(checkVoucher => checkVoucher.Supplier)
                .Where(checkVoucher =>
                    checkVoucher.PostedBy != null &&
                    checkVoucher.Date >= startDate &&
                    checkVoucher.Date < endDate)
                .OrderBy(checkVoucher => checkVoucher.Date)
                .ToListAsync(cancellationToken);

            if (checkVouchers.Count == 0)
            {
                return 0;
            }

            var accountTitlesByNumber = (await GetListOfAccountTitleDto(cancellationToken))
                .ToDictionary(account => account.AccountNumber);
            var ledgers = new List<FilprideGeneralLedgerBook>();

            foreach (var checkVoucher in checkVouchers)
            {
                if (ShouldSkipGeneralLedgerEntries(checkVoucher))
                {
                    continue;
                }

                var details = checkVoucher.Details!
                    .Where(detail => !detail.IsDisplayEntry);
                ledgers.AddRange(BuildGeneralLedgerEntries(checkVoucher, details, accountTitlesByNumber));
            }

            var references = checkVouchers
                .Select(checkVoucher => checkVoucher.CheckVoucherHeaderNo!)
                .Distinct()
                .ToList();

            await _db.FilprideGeneralLedgerBooks
                .Where(ledger => references.Contains(ledger.Reference))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.FilprideGeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return checkVouchers.Count;
        }

        private List<FilprideGeneralLedgerBook> BuildGeneralLedgerEntries(
            FilprideCheckVoucherHeader header,
            IEnumerable<FilprideCheckVoucherDetail> details,
            IReadOnlyDictionary<string, AccountTitleDto> accountTitlesByNumber)
        {
            var ledgers = new List<FilprideGeneralLedgerBook>();

            foreach (var detail in details)
            {
                if (!accountTitlesByNumber.TryGetValue(detail.AccountNo, out var account))
                {
                    throw new ArgumentException($"Account title '{detail.AccountNo}' not found.");
                }

                ledgers.Add(
                    new FilprideGeneralLedgerBook
                    {
                        Date = header.Date,
                        Reference = header.CheckVoucherHeaderNo!,
                        Description = header.Particulars!,
                        AccountId = account.AccountId,
                        AccountNo = account.AccountNumber,
                        AccountTitle = account.AccountName,
                        Debit = detail.Debit,
                        Credit = detail.Credit,
                        CreatedBy = header.PostedBy!,
                        CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                        SubAccountType = detail.SubAccountType,
                        SubAccountId = detail.SubAccountId,
                        SubAccountName = detail.SubAccountName,
                        ModuleType = nameof(ModuleType.Disbursement)
                    });
            }

            if (header.SupplierId.HasValue)
            {
                ledgers.SetCounterparty(
                    CounterpartyType.Supplier,
                    header.SupplierId,
                    header.SupplierName ?? header.Payee);
            }

            if (!IsJournalEntriesBalanced(ledgers))
            {
                throw new ArgumentException("Debit and Credit is not equal, check your entries.");
            }

            return ledgers;
        }

        private async Task<bool> ShouldSkipGeneralLedgerEntriesAsync(
            FilprideCheckVoucherHeader header,
            CancellationToken cancellationToken)
        {
            var supplierName = header.Supplier?.SupplierName;

            if (supplierName == null && header.SupplierId.HasValue)
            {
                supplierName = await _db.FilprideSuppliers
                    .AsNoTracking()
                    .Where(supplier => supplier.SupplierId == header.SupplierId.Value)
                    .Select(supplier => supplier.SupplierName)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            supplierName ??= header.SupplierName ?? header.Payee;

            return ShouldSkipGeneralLedgerEntries(supplierName);
        }

        private static bool ShouldSkipGeneralLedgerEntries(FilprideCheckVoucherHeader header)
        {
            var supplierName = header.Supplier?.SupplierName ?? header.SupplierName ?? header.Payee;
            return ShouldSkipGeneralLedgerEntries(supplierName);
        }

        private static bool ShouldSkipGeneralLedgerEntries(string? supplierName)
        {
            return supplierName?.StartsWith("MNVP", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
