using System.Linq.Expressions;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.DTOs;
using IBS.Models.Enums;
using IBS.Models.Filpride.AccountsReceivable;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Repository.Filpride
{
    public class SalesInvoiceRepository : Repository<FilprideSalesInvoice>, ISalesInvoiceRepository
    {
        private readonly ApplicationDbContext _db;

        public SalesInvoiceRepository(ApplicationDbContext db) : base(db)
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
            var lastSi = await _db
                .FilprideSalesInvoices
                .AsNoTracking()
                .OrderByDescending(x => x.SalesInvoiceNo!.Length)
                .ThenByDescending(x => x.SalesInvoiceNo)
                .FirstOrDefaultAsync(x =>
                        !x.SalesInvoiceNo!.Contains("SIBEG") &&

                        x.Type == nameof(DocumentType.Documented), cancellationToken);

            if (lastSi == null)
            {
                return "SI0000000001";
            }

            var lastSeries = lastSi.SalesInvoiceNo!;
            var numericPart = lastSeries.Substring(2);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 2) + incrementedNumber.ToString("D10");
        }

        private async Task<string> GenerateCodeForUnDocumented(CancellationToken cancellationToken)
        {
            var lastSi = await _db
                .FilprideSalesInvoices
                .AsNoTracking()
                .OrderByDescending(x => x.SalesInvoiceNo!.Length)
                .ThenByDescending(x => x.SalesInvoiceNo)
                .FirstOrDefaultAsync(x =>
                        !x.SalesInvoiceNo!.Contains("SIBEG") &&

                        x.Type == nameof(DocumentType.Undocumented), cancellationToken);

            if (lastSi == null)
            {
                return "SIU000000001";
            }

            var lastSeries = lastSi.SalesInvoiceNo!;
            var numericPart = lastSeries.Substring(3);
            var incrementedNumber = long.Parse(numericPart) + 1;

            return lastSeries.Substring(0, 3) + incrementedNumber.ToString("D9");
        }

        public async Task<SalesInvoiceTaxBalanceDto?> GetTaxBalanceAsync(int salesInvoiceId,
            int? excludedCollectionReceiptId = null,
            CancellationToken cancellationToken = default)
        {
            var salesInvoice = await _db.FilprideSalesInvoices
                .Include(si => si.Customer)
                .Include(si => si.CustomerOrderSlip)
                .FirstOrDefaultAsync(si => si.SalesInvoiceId == salesInvoiceId, cancellationToken);

            if (salesInvoice == null)
            {
                return null;
            }

            var activeDetails = _db.FilprideCollectionReceiptDetails
                .Where(detail => detail.InvoiceNo == salesInvoice.SalesInvoiceNo &&
                                 detail.FilprideCollectionReceipt != null &&
                                 (detail.FilprideCollectionReceipt.SalesInvoiceId == salesInvoiceId ||
                                  (detail.FilprideCollectionReceipt.MultipleSIId != null &&
                                   detail.FilprideCollectionReceipt.MultipleSIId.Contains(salesInvoiceId))) &&
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

            return BuildTaxBalance(salesInvoice, cwtAmountPaid, cwVatAmountPaid);
        }

        private static SalesInvoiceTaxBalanceDto BuildTaxBalance(FilprideSalesInvoice salesInvoice,
            decimal cwtAmountPaid, decimal cwVatAmountPaid)
        {
            var isVatable = (salesInvoice.CustomerOrderSlip?.VatType ?? salesInvoice.Customer?.VatType) == SD.VatType_Vatable;
            var hasEwt = salesInvoice.CustomerOrderSlip?.HasEWT ?? salesInvoice.Customer?.WithHoldingTax ?? false;
            var hasWvat = salesInvoice.CustomerOrderSlip?.HasWVAT ?? salesInvoice.Customer?.WithHoldingVat ?? false;
            var adjustedGrossAmount = salesInvoice.Amount - salesInvoice.Discount + salesInvoice.DebitAmount - salesInvoice.CreditAmount;
            var netOfVatAmount = isVatable
                ? DecimalRoundingHelper.ComputeNetOfVat(adjustedGrossAmount)
                : DecimalRoundingHelper.RoundToFour(adjustedGrossAmount);
            var cwtAmount = hasEwt
                ? DecimalRoundingHelper.ComputeEwtAmount(netOfVatAmount, salesInvoice.CwtPercent)
                : 0m;
            var cwVatAmount = hasWvat
                ? DecimalRoundingHelper.ComputeEwtAmount(netOfVatAmount, salesInvoice.CwVatPercent)
                : 0m;

            return new SalesInvoiceTaxBalanceDto
            {
                SalesInvoiceId = salesInvoice.SalesInvoiceId,
                InvoiceNo = salesInvoice.SalesInvoiceNo ?? string.Empty,
                CwtAmount = cwtAmount,
                CwtAmountPaid = cwtAmountPaid,
                CwtBalance = DecimalRoundingHelper.RoundToFour(cwtAmount - cwtAmountPaid),
                CwVatAmount = cwVatAmount,
                CwVatAmountPaid = cwVatAmountPaid,
                CwVatBalance = DecimalRoundingHelper.RoundToFour(cwVatAmount - cwVatAmountPaid)
            };
        }

        public async Task<List<SalesInvoiceCollectionDetailsDto>> GetCollectionDetailsAsync(int[] salesInvoiceIds,
            int? excludedCollectionReceiptId = null,
            CancellationToken cancellationToken = default)
        {
            var invoiceIds = salesInvoiceIds.Distinct().ToArray();
            if (invoiceIds.Length == 0)
            {
                return [];
            }

            var invoices = await _db.FilprideSalesInvoices
                .AsNoTracking()
                .Where(si => invoiceIds.Contains(si.SalesInvoiceId))
                .Select(si => new
                {
                    si.SalesInvoiceId,
                    si.SalesInvoiceNo,
                    si.Amount,
                    si.AmountPaid,
                    si.Balance,
                    si.Discount,
                    si.DebitAmount,
                    si.CreditAmount,
                    si.CwtPercent,
                    si.CwVatPercent,
                    VatType = si.CustomerOrderSlip != null ? si.CustomerOrderSlip.VatType : si.Customer!.VatType,
                    HasEwt = si.CustomerOrderSlip != null ? si.CustomerOrderSlip.HasEWT : si.Customer!.WithHoldingTax,
                    HasWvat = si.CustomerOrderSlip != null ? si.CustomerOrderSlip.HasWVAT : si.Customer!.WithHoldingVat
                })
                .ToListAsync(cancellationToken);

            var invoiceNumbers = invoices
                .Select(invoice => invoice.SalesInvoiceNo!)
                .ToArray();
            var allocations = await _db.FilprideCollectionReceiptDetails
                .AsNoTracking()
                .Where(detail => invoiceNumbers.Contains(detail.InvoiceNo) &&
                                 detail.FilprideCollectionReceipt != null &&
                                 (detail.FilprideCollectionReceipt.SalesInvoiceId != null ||
                                  detail.FilprideCollectionReceipt.MultipleSIId != null) &&
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
            var invoicesById = invoices.ToDictionary(invoice => invoice.SalesInvoiceId);

            return invoiceIds
                .Where(invoicesById.ContainsKey)
                .Select(invoiceId =>
                {
                    var invoice = invoicesById[invoiceId];
                    allocationsByInvoice.TryGetValue(invoice.SalesInvoiceNo!, out var invoiceAllocations);
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
                    var adjustedGrossAmount = invoice.Amount - invoice.Discount + invoice.DebitAmount - invoice.CreditAmount;
                    var isVatable = invoice.VatType == SD.VatType_Vatable;
                    var netOfVatAmount = isVatable
                        ? DecimalRoundingHelper.ComputeNetOfVat(adjustedGrossAmount)
                        : DecimalRoundingHelper.RoundToFour(adjustedGrossAmount);
                    var cwtAmount = invoice.HasEwt
                        ? DecimalRoundingHelper.ComputeEwtAmount(netOfVatAmount, invoice.CwtPercent)
                        : 0m;
                    var cwVatAmount = invoice.HasWvat
                        ? DecimalRoundingHelper.ComputeEwtAmount(netOfVatAmount, invoice.CwVatPercent)
                        : 0m;

                    return new SalesInvoiceCollectionDetailsDto
                    {
                        InvoiceId = invoice.SalesInvoiceId,
                        Amount = invoice.Amount,
                        AmountPaid = invoice.AmountPaid - receiptAmount,
                        NetAmount = netOfVatAmount,
                        VatAmount = isVatable ? DecimalRoundingHelper.ComputeVatAmount(netOfVatAmount) : 0m,
                        CwtBalance = DecimalRoundingHelper.RoundToFour(cwtAmount - cwtAmountPaid),
                        CwVatBalance = DecimalRoundingHelper.RoundToFour(cwVatAmount - cwVatAmountPaid),
                        Balance = invoice.Balance + receiptAmount,
                        Debit = invoice.DebitAmount,
                        Credit = invoice.CreditAmount
                    };
                })
                .ToList();
        }

        public async Task RecalculateTaxBalancesAsync(int salesInvoiceId, CancellationToken cancellationToken = default)
        {
            var taxBalance = await GetTaxBalanceAsync(salesInvoiceId, cancellationToken: cancellationToken)
                             ?? throw new InvalidOperationException("Sales invoice not found.");

            var salesInvoice = await _db.FilprideSalesInvoices
                .FirstOrDefaultAsync(si => si.SalesInvoiceId == salesInvoiceId, cancellationToken)
                ?? throw new InvalidOperationException("Sales invoice not found.");

            salesInvoice.CwtAmountPaid = taxBalance.CwtAmountPaid;
            salesInvoice.CwtBalance = taxBalance.CwtBalance;
            salesInvoice.CwVatAmountPaid = taxBalance.CwVatAmountPaid;
            salesInvoice.CwVatBalance = taxBalance.CwVatBalance;
        }

        public async Task RecalculateTaxBalancesAsync(int[] salesInvoiceIds,
            CancellationToken cancellationToken = default)
        {
            var invoiceIds = salesInvoiceIds.Distinct().ToArray();
            if (invoiceIds.Length == 0)
            {
                return;
            }

            var salesInvoices = await _db.FilprideSalesInvoices
                .Include(si => si.Customer)
                .Include(si => si.CustomerOrderSlip)
                .Where(si => invoiceIds.Contains(si.SalesInvoiceId))
                .ToListAsync(cancellationToken);
            if (salesInvoices.Count != invoiceIds.Length)
            {
                throw new InvalidOperationException("One or more sales invoices were not found.");
            }

            var invoiceNumbers = salesInvoices
                .Select(si => si.SalesInvoiceNo ?? string.Empty)
                .ToArray();
            var paidAmounts = await _db.FilprideCollectionReceiptDetails
                .Where(detail => invoiceNumbers.Contains(detail.InvoiceNo) &&
                                 detail.FilprideCollectionReceipt != null &&
                                 (detail.FilprideCollectionReceipt.SalesInvoiceId != null ||
                                  detail.FilprideCollectionReceipt.MultipleSIId != null) &&
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

            foreach (var salesInvoice in salesInvoices)
            {
                paidAmountsByInvoice.TryGetValue(salesInvoice.SalesInvoiceNo ?? string.Empty, out var paidAmount);
                var cwtAmountPaid = DecimalRoundingHelper.RoundToFour(paidAmount?.CwtAmountPaid ?? 0m);
                var cwVatAmountPaid = DecimalRoundingHelper.RoundToFour(paidAmount?.CwVatAmountPaid ?? 0m);
                var taxBalance = BuildTaxBalance(salesInvoice, cwtAmountPaid, cwVatAmountPaid);

                salesInvoice.CwtAmountPaid = taxBalance.CwtAmountPaid;
                salesInvoice.CwtBalance = taxBalance.CwtBalance;
                salesInvoice.CwVatAmountPaid = taxBalance.CwVatAmountPaid;
                salesInvoice.CwVatBalance = taxBalance.CwVatBalance;
            }
        }

        public override async Task<FilprideSalesInvoice?> GetAsync(Expression<Func<FilprideSalesInvoice, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await dbSet.Where(filter)
                .Include(si => si.Product)
                .Include(si => si.Customer)
                .Include(si => si.DeliveryReceipt)
                    .ThenInclude(dr => dr!.PurchaseOrder)
                .Include(si => si.DeliveryReceipt)
                    .ThenInclude(dr => dr!.Hauler)
                .Include(si => si.DeliveryReceipt)
                    .ThenInclude(dr => dr!.Commissionee)
                .Include(si => si.CustomerOrderSlip)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public override async Task<IEnumerable<FilprideSalesInvoice>> GetAllAsync(Expression<Func<FilprideSalesInvoice, bool>>? filter, CancellationToken cancellationToken = default)
        {
            IQueryable<FilprideSalesInvoice> query = dbSet
                .Include(si => si.Product)
                .Include(si => si.Customer)
                .Include(si => si.DeliveryReceipt).ThenInclude(dr => dr!.PurchaseOrder)
                .Include(si => si.DeliveryReceipt).ThenInclude(dr => dr!.Hauler)
                .Include(si => si.CustomerOrderSlip);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public override IQueryable<FilprideSalesInvoice> GetAllQuery(Expression<Func<FilprideSalesInvoice, bool>>? filter = null)
        {
            IQueryable<FilprideSalesInvoice> query = dbSet
                .Include(si => si.Product)
                .Include(si => si.Customer)
                .Include(si => si.DeliveryReceipt).ThenInclude(dr => dr!.PurchaseOrder)
                .Include(si => si.DeliveryReceipt).ThenInclude(dr => dr!.Hauler)
                .Include(si => si.CustomerOrderSlip)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }
    }
}
