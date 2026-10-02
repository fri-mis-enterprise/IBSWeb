using System.Linq.Expressions;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.DTOs;
using IBS.Models.Enums;
using IBS.Models.Filpride.AccountsPayable;
using IBS.Models.Filpride.Books;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Repository.Filpride
{
    public class JournalVoucherRepository: Repository<FilprideJournalVoucherHeader>, IJournalVoucherRepository
    {
        private readonly ApplicationDbContext _db;

        public JournalVoucherRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<string> GenerateCodeAsync(string? type, CancellationToken cancellationToken = default)
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
            var lastJv = await _db
                .FilprideJournalVoucherHeaders
                .AsNoTracking()
                .OrderByDescending(x => x.JournalVoucherHeaderNo!.Length)
                .ThenByDescending(x => x.JournalVoucherHeaderNo)
                .FirstOrDefaultAsync(x =>

                    x.Type == nameof(DocumentType.Documented),
                    cancellationToken);

            if (lastJv == null)
            {
                return "JV0000000001";
            }

            var lastSeries = lastJv.JournalVoucherHeaderNo!;
            var numericPart = lastSeries.Substring(2);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 2) + incrementedNumber.ToString("D10");
        }

        private async Task<string> GenerateCodeForUnDocumented(CancellationToken cancellationToken = default)
        {
            var lastJv = await _db
                .FilprideJournalVoucherHeaders
                .AsNoTracking()
                .OrderByDescending(x => x.JournalVoucherHeaderNo!.Length)
                .ThenByDescending(x => x.JournalVoucherHeaderNo)
                .FirstOrDefaultAsync(x =>

                        x.Type == nameof(DocumentType.Undocumented),
                    cancellationToken);

            if (lastJv == null)
            {
                return "JVU000000001";
            }

            var lastSeries = lastJv.JournalVoucherHeaderNo!;
            var numericPart = lastSeries.Substring(3);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 3) + incrementedNumber.ToString("D9");
        }

        public override async Task<FilprideJournalVoucherHeader?> GetAsync(Expression<Func<FilprideJournalVoucherHeader, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await dbSet.Where(filter)
                .Include(cv => cv.CheckVoucherHeader)
                .ThenInclude(supplier => supplier!.Supplier)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public override async Task<IEnumerable<FilprideJournalVoucherHeader>> GetAllAsync(Expression<Func<FilprideJournalVoucherHeader, bool>>? filter, CancellationToken cancellationToken = default)
        {
            IQueryable<FilprideJournalVoucherHeader> query = dbSet
                .Include(cv => cv.CheckVoucherHeader)
                .ThenInclude(supplier => supplier!.Supplier);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public override IQueryable<FilprideJournalVoucherHeader> GetAllQuery(Expression<Func<FilprideJournalVoucherHeader, bool>>? filter = null)
        {
            IQueryable<FilprideJournalVoucherHeader> query =
                dbSet
                .Include(cv => cv.CheckVoucherHeader)
                .ThenInclude(supplier => supplier!.Supplier)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        public async Task PostAsync(FilprideJournalVoucherHeader header,
            IEnumerable<FilprideJournalVoucherDetail> details,
            CancellationToken cancellationToken = default,
            List<AccountTitleDto>? accountTitlesDto = null,
            bool saveChanges = true)
        {
            #region --General Ledger Book Recording(GL)--

            accountTitlesDto ??= await GetListOfAccountTitleDto(cancellationToken);
            var ledgers = new List<FilprideGeneralLedgerBook>();

            foreach (var detail in details)
            {
                var account = accountTitlesDto.Find(c => c.AccountNumber == detail.AccountNo) ?? throw new ArgumentException($"Account title '{detail.AccountNo}' not found.");
                ledgers.Add(
                    new FilprideGeneralLedgerBook
                    {
                        Date = header.Date,
                        Reference = header.JournalVoucherHeaderNo!,
                        Description = header.Particulars,
                        AccountId = account.AccountId,
                        AccountNo = account.AccountNumber,
                        AccountTitle = account.AccountName,
                        Debit = detail.Debit,
                        Credit = detail.Credit,
                        CreatedBy = header.CreatedBy!,
                        CreatedDate = header.CreatedDate,
                        SubAccountType = detail.SubAccountType,
                        SubAccountId = detail.SubAccountId,
                        SubAccountName = detail.SubAccountName,
                        ModuleType = nameof(ModuleType.Journal)
                    }
                );
            }

            if (header.CVId.HasValue)
            {
                var supplierId = header.CheckVoucherHeader?.SupplierId;
                var supplierName = header.CheckVoucherHeader?.SupplierName ?? header.CheckVoucherHeader?.Payee;

                if (!supplierId.HasValue)
                {
                    var counterparty = await _db.FilprideCheckVoucherHeaders
                        .AsNoTracking()
                        .Where(checkVoucher => checkVoucher.CheckVoucherHeaderId == header.CVId.Value)
                        .Select(checkVoucher => new
                        {
                            checkVoucher.SupplierId,
                            Name = checkVoucher.SupplierName ?? checkVoucher.Payee
                        })
                        .SingleOrDefaultAsync(cancellationToken);
                    supplierId = counterparty?.SupplierId;
                    supplierName = counterparty?.Name;
                }

                if (supplierId.HasValue)
                {
                    ledgers.SetCounterparty(
                        CounterpartyType.Supplier,
                        supplierId,
                        supplierName);
                }
            }

            if (!IsJournalEntriesBalanced(ledgers))
            {
                throw new ArgumentException("Debit and Credit is not equal, check your entries.");
            }

            await _db.FilprideGeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);

            #endregion --General Ledger Book Recording(GL)--

            if (saveChanges)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
