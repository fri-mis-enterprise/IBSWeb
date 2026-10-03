using System.Linq.Expressions;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.DTOs;
using IBS.Models.Enums;
using IBS.Models.Filpride.AccountsReceivable;
using IBS.Models.Filpride.Books;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Repository.Filpride
{
    public class ServiceInvoiceRepository: Repository<FilprideServiceInvoice>, IServiceInvoiceRepository
    {
        private readonly ApplicationDbContext _db;

        public ServiceInvoiceRepository(ApplicationDbContext db) : base(db)
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

        private async Task<string> GenerateCodeForDocumented(CancellationToken cancellationToken)
        {
            var lastSv = await _db
                .FilprideServiceInvoices
                .AsNoTracking()
                .OrderByDescending(x => x.ServiceInvoiceNo!.Length)
                .ThenByDescending(x => x.ServiceInvoiceNo)
                .FirstOrDefaultAsync(x =>

                    x.Type == nameof(DocumentType.Documented),
                    cancellationToken);

            if (lastSv == null)
            {
                return "SV0000000001";
            }

            var lastSeries = lastSv.ServiceInvoiceNo;
            var numericPart = lastSeries.Substring(2);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 2) + incrementedNumber.ToString("D10");
        }

        private async Task<string> GenerateCodeForUnDocumented(CancellationToken cancellationToken)
        {
            var lastSv = await _db
                .FilprideServiceInvoices
                .AsNoTracking()
                .OrderByDescending(x => x.ServiceInvoiceNo!.Length)
                .ThenByDescending(x => x.ServiceInvoiceNo)
                .FirstOrDefaultAsync(x =>

                        x.Type == nameof(DocumentType.Undocumented),
                    cancellationToken);

            if (lastSv == null)
            {
                return "SVU000000001";
            }

            var lastSeries = lastSv.ServiceInvoiceNo;
            var numericPart = lastSeries.Substring(3);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 3) + incrementedNumber.ToString("D9");
        }

        public async Task<ServiceInvoiceTaxBalanceDto?> GetTaxBalanceAsync(int serviceInvoiceId,
            int? excludedCollectionReceiptId = null,
            CancellationToken cancellationToken = default)
        {
            var serviceInvoice = await _db.FilprideServiceInvoices
                .FirstOrDefaultAsync(sv => sv.ServiceInvoiceId == serviceInvoiceId, cancellationToken);

            if (serviceInvoice == null)
            {
                return null;
            }

            var activeDetails = _db.FilprideCollectionReceiptDetails
                .Where(detail => detail.InvoiceNo == serviceInvoice.ServiceInvoiceNo &&
                                 detail.FilprideCollectionReceipt != null &&
                                 (detail.FilprideCollectionReceipt.ServiceInvoiceId == serviceInvoiceId ||
                                  (detail.FilprideCollectionReceipt.MultipleSVId != null &&
                                   detail.FilprideCollectionReceipt.MultipleSVId.Contains(serviceInvoiceId))) &&
                                 detail.FilprideCollectionReceipt.Status != nameof(CollectionReceiptStatus.Canceled) &&
                                 detail.FilprideCollectionReceipt.Status != nameof(CollectionReceiptStatus.Voided));

            if (excludedCollectionReceiptId.HasValue)
            {
                activeDetails = activeDetails.Where(detail => detail.CollectionReceiptId != excludedCollectionReceiptId.Value);
            }

            var paidAmounts = await activeDetails
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    CwtAmountPaid = group.Sum(detail => detail.EWT),
                    CwVatAmountPaid = group.Sum(detail => detail.WVAT)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var cwtAmountPaid = DecimalRoundingHelper.RoundToFour(paidAmounts?.CwtAmountPaid ?? 0m);
            var cwVatAmountPaid = DecimalRoundingHelper.RoundToFour(paidAmounts?.CwVatAmountPaid ?? 0m);

            return BuildTaxBalance(serviceInvoice, cwtAmountPaid, cwVatAmountPaid);
        }

        private static ServiceInvoiceTaxBalanceDto BuildTaxBalance(FilprideServiceInvoice serviceInvoice,
            decimal cwtAmountPaid, decimal cwVatAmountPaid)
        {
            var adjustedGrossAmount = serviceInvoice.Total - serviceInvoice.Discount
                                      + serviceInvoice.DebitAmount - serviceInvoice.CreditAmount;
            var netOfVatAmount = serviceInvoice.VatType == SD.VatType_Vatable
                ? DecimalRoundingHelper.ComputeNetOfVat(adjustedGrossAmount)
                : DecimalRoundingHelper.RoundToFour(adjustedGrossAmount);
            var cwtAmount = serviceInvoice.HasEwt
                ? DecimalRoundingHelper.ComputeEwtAmount(netOfVatAmount, serviceInvoice.ServicePercent / 100m)
                : 0m;
            var cwVatAmount = serviceInvoice.HasWvat
                ? DecimalRoundingHelper.ComputeEwtAmount(netOfVatAmount, 0.05m)
                : 0m;

            return new ServiceInvoiceTaxBalanceDto
            {
                ServiceInvoiceId = serviceInvoice.ServiceInvoiceId,
                InvoiceNo = serviceInvoice.ServiceInvoiceNo,
                CwtAmount = cwtAmount,
                CwtAmountPaid = cwtAmountPaid,
                CwtBalance = DecimalRoundingHelper.RoundToFour(cwtAmount - cwtAmountPaid),
                CwVatAmount = cwVatAmount,
                CwVatAmountPaid = cwVatAmountPaid,
                CwVatBalance = DecimalRoundingHelper.RoundToFour(cwVatAmount - cwVatAmountPaid)
            };
        }

        public async Task<List<ServiceInvoiceCollectionDetailsDto>> GetCollectionDetailsAsync(int[] serviceInvoiceIds,
            int? excludedCollectionReceiptId = null,
            CancellationToken cancellationToken = default)
        {
            var invoiceIds = serviceInvoiceIds.Distinct().ToArray();
            if (invoiceIds.Length == 0)
            {
                return [];
            }

            var invoices = await _db.FilprideServiceInvoices
                .AsNoTracking()
                .Where(sv => invoiceIds.Contains(sv.ServiceInvoiceId))
                .Select(sv => new
                {
                    sv.ServiceInvoiceId,
                    sv.ServiceInvoiceNo,
                    sv.Total,
                    sv.AmountPaid,
                    sv.Balance,
                    sv.Discount,
                    sv.DebitAmount,
                    sv.CreditAmount,
                    sv.VatType,
                    sv.HasEwt,
                    sv.HasWvat,
                    sv.ServicePercent
                })
                .ToListAsync(cancellationToken);

            var invoiceNumbers = invoices
                .Select(invoice => invoice.ServiceInvoiceNo)
                .ToArray();
            var allocations = await _db.FilprideCollectionReceiptDetails
                .AsNoTracking()
                .Where(detail => invoiceNumbers.Contains(detail.InvoiceNo) &&
                                 detail.FilprideCollectionReceipt != null &&
                                 (detail.FilprideCollectionReceipt.ServiceInvoiceId != null ||
                                  detail.FilprideCollectionReceipt.MultipleSVId != null) &&
                                 detail.FilprideCollectionReceipt.Status != nameof(CollectionReceiptStatus.Canceled) &&
                                 detail.FilprideCollectionReceipt.Status != nameof(CollectionReceiptStatus.Voided))
                .Select(detail => new
                {
                    detail.InvoiceNo,
                    detail.CollectionReceiptId,
                    detail.Amount,
                    detail.EWT,
                    detail.WVAT
                })
                .ToListAsync(cancellationToken);

            var allocationsByInvoice = allocations
                .GroupBy(allocation => allocation.InvoiceNo, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, StringComparer.OrdinalIgnoreCase);
            var invoicesById = invoices.ToDictionary(invoice => invoice.ServiceInvoiceId);

            return invoiceIds
                .Where(invoicesById.ContainsKey)
                .Select(invoiceId =>
                {
                    var invoice = invoicesById[invoiceId];
                    allocationsByInvoice.TryGetValue(invoice.ServiceInvoiceNo, out var invoiceAllocations);
                    var receiptAmount = excludedCollectionReceiptId.HasValue
                        ? invoiceAllocations?.Where(allocation => allocation.CollectionReceiptId == excludedCollectionReceiptId.Value)
                            .Sum(allocation => allocation.Amount) ?? 0m
                        : 0m;
                    var cwtAmountPaid = DecimalRoundingHelper.RoundToFour(invoiceAllocations?
                        .Where(allocation => allocation.CollectionReceiptId != excludedCollectionReceiptId)
                        .Sum(allocation => allocation.EWT) ?? 0m);
                    var cwVatAmountPaid = DecimalRoundingHelper.RoundToFour(invoiceAllocations?
                        .Where(allocation => allocation.CollectionReceiptId != excludedCollectionReceiptId)
                        .Sum(allocation => allocation.WVAT) ?? 0m);
                    var adjustedGrossAmount = invoice.Total - invoice.Discount + invoice.DebitAmount - invoice.CreditAmount;
                    var netOfVatAmount = invoice.VatType == SD.VatType_Vatable
                        ? DecimalRoundingHelper.ComputeNetOfVat(adjustedGrossAmount)
                        : DecimalRoundingHelper.RoundToFour(adjustedGrossAmount);
                    var cwtAmount = invoice.HasEwt
                        ? DecimalRoundingHelper.ComputeEwtAmount(netOfVatAmount, invoice.ServicePercent / 100m)
                        : 0m;
                    var cwVatAmount = invoice.HasWvat
                        ? DecimalRoundingHelper.ComputeEwtAmount(netOfVatAmount, 0.05m)
                        : 0m;

                    return new ServiceInvoiceCollectionDetailsDto
                    {
                        InvoiceId = invoice.ServiceInvoiceId,
                        Amount = invoice.Total,
                        AmountPaid = invoice.AmountPaid - receiptAmount,
                        NetAmount = netOfVatAmount,
                        CwtBalance = DecimalRoundingHelper.RoundToFour(cwtAmount - cwtAmountPaid),
                        CwVatBalance = DecimalRoundingHelper.RoundToFour(cwVatAmount - cwVatAmountPaid),
                        Balance = invoice.Balance + receiptAmount,
                        Debit = invoice.DebitAmount,
                        Credit = invoice.CreditAmount
                    };
                })
                .ToList();
        }

        public async Task RecalculateTaxBalancesAsync(int serviceInvoiceId, CancellationToken cancellationToken = default)
        {
            var taxBalance = await GetTaxBalanceAsync(serviceInvoiceId, cancellationToken: cancellationToken)
                             ?? throw new InvalidOperationException("Service invoice not found.");

            var serviceInvoice = await _db.FilprideServiceInvoices
                .FirstOrDefaultAsync(sv => sv.ServiceInvoiceId == serviceInvoiceId, cancellationToken)
                ?? throw new InvalidOperationException("Service invoice not found.");

            serviceInvoice.CwtAmountPaid = taxBalance.CwtAmountPaid;
            serviceInvoice.CwtBalance = taxBalance.CwtBalance;
            serviceInvoice.CwVatAmountPaid = taxBalance.CwVatAmountPaid;
            serviceInvoice.CwVatBalance = taxBalance.CwVatBalance;
        }

        public async Task RecalculateTaxBalancesAsync(int[] serviceInvoiceIds,
            CancellationToken cancellationToken = default)
        {
            var invoiceIds = serviceInvoiceIds.Distinct().ToArray();
            if (invoiceIds.Length == 0)
            {
                return;
            }

            var serviceInvoices = await _db.FilprideServiceInvoices
                .Where(sv => invoiceIds.Contains(sv.ServiceInvoiceId))
                .ToListAsync(cancellationToken);
            if (serviceInvoices.Count != invoiceIds.Length)
            {
                throw new InvalidOperationException("One or more service invoices were not found.");
            }

            var invoiceNumbers = serviceInvoices
                .Select(sv => sv.ServiceInvoiceNo)
                .ToArray();
            var paidAmounts = await _db.FilprideCollectionReceiptDetails
                .Where(detail => invoiceNumbers.Contains(detail.InvoiceNo) &&
                                 detail.FilprideCollectionReceipt != null &&
                                 (detail.FilprideCollectionReceipt.ServiceInvoiceId != null ||
                                  detail.FilprideCollectionReceipt.MultipleSVId != null) &&
                                 detail.FilprideCollectionReceipt.Status != nameof(CollectionReceiptStatus.Canceled) &&
                                 detail.FilprideCollectionReceipt.Status != nameof(CollectionReceiptStatus.Voided))
                .GroupBy(detail => detail.InvoiceNo)
                .Select(group => new
                {
                    InvoiceNo = group.Key,
                    CwtAmountPaid = group.Sum(detail => detail.EWT),
                    CwVatAmountPaid = group.Sum(detail => detail.WVAT)
                })
                .ToListAsync(cancellationToken);
            var paidAmountsByInvoice = paidAmounts.ToDictionary(amount => amount.InvoiceNo);

            foreach (var serviceInvoice in serviceInvoices)
            {
                paidAmountsByInvoice.TryGetValue(serviceInvoice.ServiceInvoiceNo, out var paidAmount);
                var cwtAmountPaid = DecimalRoundingHelper.RoundToFour(paidAmount?.CwtAmountPaid ?? 0m);
                var cwVatAmountPaid = DecimalRoundingHelper.RoundToFour(paidAmount?.CwVatAmountPaid ?? 0m);
                var taxBalance = BuildTaxBalance(serviceInvoice, cwtAmountPaid, cwVatAmountPaid);

                serviceInvoice.CwtAmountPaid = taxBalance.CwtAmountPaid;
                serviceInvoice.CwtBalance = taxBalance.CwtBalance;
                serviceInvoice.CwVatAmountPaid = taxBalance.CwVatAmountPaid;
                serviceInvoice.CwVatBalance = taxBalance.CwVatBalance;
            }
        }

        public override async Task<FilprideServiceInvoice?> GetAsync(Expression<Func<FilprideServiceInvoice, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await dbSet.Where(filter)
                .Include(s => s.Customer)
                .Include(s => s.Service)
                .Include(s => s.DeliveryReceipt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public override async Task<IEnumerable<FilprideServiceInvoice>> GetAllAsync(Expression<Func<FilprideServiceInvoice, bool>>? filter, CancellationToken cancellationToken = default)
        {
            IQueryable<FilprideServiceInvoice> query = dbSet
                .Include(s => s.Customer)
                .Include(s => s.Service)
                .Include(s => s.DeliveryReceipt);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public override IQueryable<FilprideServiceInvoice> GetAllQuery(Expression<Func<FilprideServiceInvoice, bool>>? filter = null)
        {
            IQueryable<FilprideServiceInvoice> query =
                dbSet
                .Include(s => s.Customer)
                .Include(s => s.Service)
                .Include(s => s.DeliveryReceipt)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        public async Task PostAsync(FilprideServiceInvoice model,
            CancellationToken cancellationToken = default,
            List<AccountTitleDto>? accountTitlesDto = null)
        {
            #region --Sales Book Recording

            decimal withHoldingTaxAmount = 0;
            decimal withHoldingVatAmount = 0;
            decimal netOfVatAmount;
            decimal vatAmount = 0;

            if (model.VatType == SD.VatType_Vatable)
            {
                netOfVatAmount = ComputeNetOfVat(model.Total);
                vatAmount = ComputeVatAmount(netOfVatAmount);
            }
            else
            {
                netOfVatAmount = model.Total;
            }

            if (model.HasEwt)
            {
                withHoldingTaxAmount = ComputeEwtAmount(netOfVatAmount, model.Service!.Percent / 100m);
            }

            if (model.HasWvat)
            {
                withHoldingVatAmount = ComputeEwtAmount(netOfVatAmount, 0.05m);
            }

            var ledgers = new List<FilprideGeneralLedgerBook>();
            accountTitlesDto ??= await GetListOfAccountTitleDto(cancellationToken);
            var arTradeTitle = accountTitlesDto.Find(c => c.AccountNumber == "101020100")
                               ?? throw new ArgumentException("Account title '101020100' not found.");
            var arTradeCwt = accountTitlesDto.Find(c => c.AccountNumber == "101020200")
                             ?? throw new ArgumentException("Account title '101020200' not found.");
            var arTradeCwv = accountTitlesDto.Find(c => c.AccountNumber == "101020300")
                             ?? throw new ArgumentException("Account title '101020300' not found.");
            var vatOutputTitle = accountTitlesDto.Find(c => c.AccountNumber == "201030100")
                                 ?? throw new ArgumentException("Account title '201030100' not found.");
            var currentServiceAccount = accountTitlesDto.Find(c => c.AccountNumber == model.Service!.CurrentAndPreviousNo!)
                                ?? throw new ArgumentException($"Account title '{model.Service!.CurrentAndPreviousNo}' not found.");
            var unearnedServiceAccount = accountTitlesDto.Find(c => c.AccountNumber == model.Service!.UnearnedNo!)
                                        ?? throw new ArgumentException($"Account title '{model.Service!.UnearnedNo}' not found.");

            var period = model.Period;
            var today = DateTimeHelper.GetCurrentPhilippineTime();
            var lastDayOfTheMonth = DateTimeHelper.GetLastDayOfMonth();
            var isCurrent = period <= lastDayOfTheMonth;
            var particulars = $"{model.ServiceName} for the period of {model.Period:MMM yyyy}";

            ledgers.Add(
                new FilprideGeneralLedgerBook
                {
                    Date = isCurrent ? period : DateOnly.FromDateTime(today),
                    Reference = model.ServiceInvoiceNo,
                    Description = particulars,
                    AccountId = arTradeTitle.AccountId,
                    AccountNo = arTradeTitle.AccountNumber,
                    AccountTitle = arTradeTitle.AccountName,
                    Debit = model.Total - (withHoldingTaxAmount + withHoldingVatAmount),
                    Credit = 0,
                    CreatedBy = model.PostedBy!,
                    CreatedDate = today,
                    SubAccountType = SubAccountType.Customer,
                    SubAccountId = model.CustomerId,
                    SubAccountName = model.CustomerName,
                    ModuleType = nameof(ModuleType.Sales)
                }
            );
            if (withHoldingTaxAmount > 0)
            {
                ledgers.Add(
                    new FilprideGeneralLedgerBook
                    {
                        Date = isCurrent ? period : DateOnly.FromDateTime(today),
                        Reference = model.ServiceInvoiceNo,
                        Description = particulars,
                        AccountId = arTradeCwt.AccountId,
                        AccountNo = arTradeCwt.AccountNumber,
                        AccountTitle = arTradeCwt.AccountName,
                        Debit = withHoldingTaxAmount,
                        Credit = 0,
                        CreatedBy = model.PostedBy!,
                        CreatedDate = today,
                        ModuleType = nameof(ModuleType.Sales)
                    }
                );
            }
            if (withHoldingVatAmount > 0)
            {
                ledgers.Add(
                    new FilprideGeneralLedgerBook
                    {
                        Date = isCurrent ? period : DateOnly.FromDateTime(today),
                        Reference = model.ServiceInvoiceNo,
                        Description = particulars,
                        AccountId = arTradeCwv.AccountId,
                        AccountNo = arTradeCwv.AccountNumber,
                        AccountTitle = arTradeCwv.AccountName,
                        Debit = withHoldingVatAmount,
                        Credit = 0,
                        CreatedBy = model.PostedBy!,
                        CreatedDate = today,
                        ModuleType = nameof(ModuleType.Sales)
                    }
                );
            }

            ledgers.Add(
                new FilprideGeneralLedgerBook
                {
                    Date = isCurrent ? period : DateOnly.FromDateTime(today),
                    Reference = model.ServiceInvoiceNo,
                    Description = particulars,
                    AccountId = isCurrent ? currentServiceAccount.AccountId : unearnedServiceAccount.AccountId,
                    AccountNo = isCurrent ? currentServiceAccount.AccountNumber : unearnedServiceAccount.AccountNumber,
                    AccountTitle = isCurrent ? currentServiceAccount.AccountName : unearnedServiceAccount.AccountName,
                    Debit = 0,
                    Credit = netOfVatAmount,
                    CreatedBy = model.PostedBy!,
                    CreatedDate = today,
                    ModuleType = nameof(ModuleType.Sales)
                }
            );

            if (vatAmount > 0)
            {
                ledgers.Add(
                    new FilprideGeneralLedgerBook
                    {
                        Date = isCurrent ? period : DateOnly.FromDateTime(today),
                        Reference = model.ServiceInvoiceNo,
                        Description = particulars,
                        AccountId = vatOutputTitle.AccountId,
                        AccountNo = vatOutputTitle.AccountNumber,
                        AccountTitle = vatOutputTitle.AccountName,
                        Debit = 0,
                        Credit = vatAmount,
                        CreatedBy = model.PostedBy!,
                        CreatedDate = today,
                        ModuleType = nameof(ModuleType.Sales)
                    }
                );
            }

            if (!isCurrent)
            {
                ledgers.Add(
                    new FilprideGeneralLedgerBook
                    {
                        Date = period,
                        Reference = model.ServiceInvoiceNo,
                        Description = particulars,
                        AccountId = unearnedServiceAccount.AccountId,
                        AccountNo = unearnedServiceAccount.AccountNumber,
                        AccountTitle = unearnedServiceAccount.AccountName,
                        Debit = netOfVatAmount,
                        Credit = 0,
                        CreatedBy = model.PostedBy!,
                        CreatedDate = today,
                        ModuleType = nameof(ModuleType.Sales)
                    }
                );

                ledgers.Add(
                    new FilprideGeneralLedgerBook
                    {
                        Date = period,
                        Reference = model.ServiceInvoiceNo,
                        Description = particulars,
                        AccountId = currentServiceAccount.AccountId,
                        AccountNo = currentServiceAccount.AccountNumber,
                        AccountTitle = currentServiceAccount.AccountName,
                        Debit = 0,
                        Credit = netOfVatAmount,
                        CreatedBy = model.PostedBy!,
                        CreatedDate = today,
                        ModuleType = nameof(ModuleType.Sales)
                    }
                );
            }

            ledgers.SetCounterparty(
                CounterpartyType.Customer,
                model.CustomerId,
                model.CustomerName);

            if (!IsJournalEntriesBalanced(ledgers))
            {
                throw new ArgumentException("Debit and Credit is not equal, check your entries.");
            }

            await _db.FilprideGeneralLedgerBooks.AddRangeAsync(ledgers, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            #endregion --General Ledger Book Recording
        }
    }
}
