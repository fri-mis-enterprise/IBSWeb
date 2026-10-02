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
