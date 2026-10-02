using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.DTOs;
using IBS.Models;
using IBS.Models.Enums;
using IBS.Models.Filpride.AccountsReceivable;
using IBS.Models.Filpride.Books;
using IBS.Models.Filpride.ViewModels;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IBS.Services
{
    public interface ITransactionMasterControlService
    {
        Task<(string Type, string ReferenceNo)?> FindTransactionAsync(string referenceNo, CancellationToken cancellationToken);
        Task<TransactionMasterControlViewModel?> GetTransactionDetailsAsync(string referenceNo, string type, CancellationToken cancellationToken);
        Task UpdateTransactionAsync(TransactionMasterControlViewModel model, string userFullName, CancellationToken cancellationToken);
        Task<ReJournalBatchResult> ReJournalAllAsync(int month, int year, string userFullName, string transactionType, CancellationToken cancellationToken);
    }

    public sealed class ReJournalBatchResult
    {
        public int PurchaseCount { get; init; }
        public int SalesCount { get; init; }
        public int ServiceCount { get; init; }
        public int CollectionCount { get; init; }
        public int ProvisionalReceiptCount { get; init; }
        public int DebitMemoCount { get; init; }
        public int CreditMemoCount { get; init; }
        public int PaymentCount { get; init; }
        public int JvCount { get; init; }
    }

    public class TransactionMasterControlService(
        ApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        ILogger<TransactionMasterControlService> logger)
        : ITransactionMasterControlService
    {
        private const string _paymentForSeparator = ". Payment for ";
        private const string _dateFormat = "MM/dd/yyyy";
        public const string ReJournalTypeAll = "All";
        public const string ReJournalTypePurchase = "Purchase";
        public const string ReJournalTypeSales = "Sales";
        public const string ReJournalTypeService = "Service";
        public const string ReJournalTypeCollection = "Collection";
        public const string ReJournalTypeProvisionalReceipt = "ProvisionalReceipt";
        public const string ReJournalTypeDebitMemo = "DebitMemo";
        public const string ReJournalTypeCreditMemo = "CreditMemo";
        public const string ReJournalTypePayment = "Payment";
        public const string ReJournalTypeJv = "JV";

        public static readonly IReadOnlyList<string> ReJournalTypes =
        [
            ReJournalTypeAll,
            ReJournalTypePurchase,
            ReJournalTypeSales,
            ReJournalTypeService,
            ReJournalTypeCollection,
            ReJournalTypeProvisionalReceipt,
            ReJournalTypeDebitMemo,
            ReJournalTypeCreditMemo,
            ReJournalTypePayment,
            ReJournalTypeJv
        ];

        public async Task<(string Type, string ReferenceNo)?> FindTransactionAsync(string referenceNo, CancellationToken cancellationToken)
        {
            referenceNo = referenceNo.Trim();

            if (await dbContext.FilprideCheckVoucherHeaders.AnyAsync(x => x.CheckVoucherHeaderNo == referenceNo, cancellationToken))
            {
                return ("CV", referenceNo);
            }

            if (await dbContext.FilprideJournalVoucherHeaders.AnyAsync(x => x.JournalVoucherHeaderNo == referenceNo, cancellationToken))
            {
                return ("JV", referenceNo);
            }

            if (await dbContext.FilprideSalesInvoices.AnyAsync(x => x.SalesInvoiceNo == referenceNo, cancellationToken))
            {
                return ("SI", referenceNo);
            }

            if (await dbContext.FilprideServiceInvoices.AnyAsync(x => x.ServiceInvoiceNo == referenceNo, cancellationToken))
            {
                return ("SV", referenceNo);
            }

            if (await dbContext.FilprideCollectionReceipts.AnyAsync(x => x.CollectionReceiptNo == referenceNo, cancellationToken))
            {
                return ("CR", referenceNo);
            }

            return null;
        }

        public async Task<TransactionMasterControlViewModel?> GetTransactionDetailsAsync(string referenceNo, string type, CancellationToken cancellationToken)
        {
            TransactionMasterControlViewModel model = new() { ReferenceNo = referenceNo, TransactionType = type };

            if (type == "CV")
            {
                var header = await dbContext.FilprideCheckVoucherHeaders
                    .FirstOrDefaultAsync(x => x.CheckVoucherHeaderNo == referenceNo, cancellationToken);

                if (header == null)
                {
                    return null;
                }

                model.Date = header.Date;
                var particulars = header.Particulars ?? string.Empty;
                var index = particulars.IndexOf(_paymentForSeparator, StringComparison.Ordinal);
                if (index >= 0)
                {
                    model.Particulars = particulars.Substring(0, index).Trim();
                    model.PaymentFor = particulars.Substring(index + _paymentForSeparator.Length).Trim();
                }
                else
                {
                    model.Particulars = particulars;
                }

                model.Payee = header.Payee;
                model.CheckNo = header.CheckNo;
                model.CheckDate = header.CheckDate;
                model.IsFound = true;
            }
            else if (type == "JV")
            {
                var header = await dbContext.FilprideJournalVoucherHeaders
                    .FirstOrDefaultAsync(x => x.JournalVoucherHeaderNo == referenceNo, cancellationToken);

                if (header == null)
                {
                    return null;
                }

                model.Date = header.Date;
                model.Particulars = header.Particulars;
                model.IsFound = true;
            }
            else if (type == "SI")
            {
                var header = await dbContext.FilprideSalesInvoices
                    .FirstOrDefaultAsync(x => x.SalesInvoiceNo == referenceNo, cancellationToken);

                if (header == null)
                {
                    return null;
                }

                model.Date = header.TransactionDate;
                model.Particulars = header.Remarks;
                model.IsFound = true;
            }
            else if (type == "SV")
            {
                var header = await dbContext.FilprideServiceInvoices
                    .FirstOrDefaultAsync(x => x.ServiceInvoiceNo == referenceNo, cancellationToken);

                if (header == null)
                {
                    return null;
                }

                model.Date = header.Period;
                model.Particulars = header.Instructions;
                model.IsFound = true;
            }
            else if (type == "CR")
            {
                var header = await dbContext.FilprideCollectionReceipts
                    .FirstOrDefaultAsync(x => x.CollectionReceiptNo == referenceNo, cancellationToken);

                if (header == null)
                {
                    return null;
                }

                model.Date = header.TransactionDate;
                model.Particulars = header.Remarks ?? string.Empty;
                model.IsFound = true;
            }
            else
            {
                return null;
            }

            return model;
        }

        public async Task UpdateTransactionAsync(TransactionMasterControlViewModel model, string userFullName, CancellationToken cancellationToken)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (model.TransactionType == "CV")
                {
                    var header = await dbContext.FilprideCheckVoucherHeaders
                        .FirstOrDefaultAsync(x => x.CheckVoucherHeaderNo == model.ReferenceNo, cancellationToken);

                    if (header == null)
                    {
                        throw new InvalidOperationException("CV Header not found.");
                    }

                    var finalParticulars = !string.IsNullOrWhiteSpace(model.PaymentFor)
                        ? $"{model.Particulars}{_paymentForSeparator}{model.PaymentFor}"
                        : model.Particulars;

                    header.Particulars = finalParticulars;
                    header.Payee = model.Payee;
                    header.CheckNo = model.CheckNo;
                    header.CheckDate = model.CheckDate;
                    StampEdited(header, userFullName);

                    await UpdateGeneralLedgerBooksAsync(model.ReferenceNo, finalParticulars, cancellationToken);

                    if (header.CvType == nameof(CVType.Invoicing))
                    {
                        var paymentCvIds = await dbContext.FilprideMultipleCheckVoucherPayments
                            .Where(x => x.CheckVoucherHeaderInvoiceId == header.CheckVoucherHeaderId)
                            .Select(x => x.CheckVoucherHeaderPaymentId)
                            .ToListAsync(cancellationToken);

                        var paymentHeaders = await dbContext.FilprideCheckVoucherHeaders
                            .Where(x => paymentCvIds.Contains(x.CheckVoucherHeaderId))
                            .ToDictionaryAsync(x => x.CheckVoucherHeaderId, cancellationToken);

                        foreach (var paymentId in paymentCvIds)
                        {
                            if (!paymentHeaders.TryGetValue(paymentId, out var paymentHeader))
                            {
                                continue;
                            }

                            var oldPaymentParticulars = paymentHeader.Particulars ?? "";
                            var paymentIndex = oldPaymentParticulars.IndexOf(_paymentForSeparator, StringComparison.Ordinal);

                            if (paymentIndex < 0)
                            {
                                continue;
                            }

                            var suffix = oldPaymentParticulars.Substring(paymentIndex);
                            var newPaymentParticulars = model.Particulars + suffix;

                            if (paymentHeader.Particulars == newPaymentParticulars)
                            {
                                continue;
                            }

                            paymentHeader.Particulars = newPaymentParticulars;
                            StampEdited(paymentHeader, userFullName);

                            await UpdateGeneralLedgerBooksAsync(paymentHeader.CheckVoucherHeaderNo!, newPaymentParticulars, cancellationToken);
                        }
                    }
                }
                else if (model.TransactionType == "JV")
                {
                    var header = await dbContext.FilprideJournalVoucherHeaders
                        .FirstOrDefaultAsync(x => x.JournalVoucherHeaderNo == model.ReferenceNo, cancellationToken);
                    if (header == null)
                    {
                        throw new InvalidOperationException("JV Header not found.");
                    }

                    header.Particulars = model.Particulars;
                    StampEdited(header, userFullName);

                    await UpdateGeneralLedgerBooksAsync(model.ReferenceNo, model.Particulars, cancellationToken);
                }
                else if (model.TransactionType == "SI")
                {
                    var header = await dbContext.FilprideSalesInvoices
                        .FirstOrDefaultAsync(x => x.SalesInvoiceNo == model.ReferenceNo, cancellationToken);
                    if (header == null)
                    {
                        throw new InvalidOperationException("SI Header not found.");
                    }

                    header.Remarks = model.Particulars;
                    StampEdited(header, userFullName);

                }
                else if (model.TransactionType == "SV")
                {
                    var header = await dbContext.FilprideServiceInvoices
                        .FirstOrDefaultAsync(x => x.ServiceInvoiceNo == model.ReferenceNo, cancellationToken);
                    if (header == null)
                    {
                        throw new InvalidOperationException("SV Header not found.");
                    }

                    header.Instructions = model.Particulars;
                    StampEdited(header, userFullName);

                }
                else if (model.TransactionType == "CR")
                {
                    var header = await dbContext.FilprideCollectionReceipts
                        .FirstOrDefaultAsync(x => x.CollectionReceiptNo == model.ReferenceNo, cancellationToken);
                    if (header == null)
                    {
                        throw new InvalidOperationException("CR Header not found.");
                    }

                    header.Remarks = model.Particulars;
                    StampEdited(header, userFullName);

                }

                await dbContext.SaveChangesAsync(cancellationToken);

                FilprideAuditTrail auditTrail = new(
                    userFullName,
                    $"Updated particulars/metadata for {model.TransactionType}# {model.ReferenceNo} via Master Control",
                    "Master Control"
                );
                await unitOfWork.FilprideAuditTrail.AddAsync(auditTrail, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                var safeRefNo = model.ReferenceNo.Replace("\r", string.Empty).Replace("\n", string.Empty);
                logger.LogError(ex, "Error updating transaction via Master Control. Ref: {Ref}", safeRefNo);
                throw;
            }
        }

        private static void StampEdited(BaseEntity header, string userFullName)
        {
            header.EditedBy = userFullName;
            header.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();
        }

        public async Task<ReJournalBatchResult> ReJournalAllAsync(int month, int year, string userFullName, string transactionType, CancellationToken cancellationToken)
        {
            transactionType = string.IsNullOrWhiteSpace(transactionType)
                ? ReJournalTypeAll
                : transactionType.Trim();

            if (!ReJournalTypes.Contains(transactionType, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid rejournal type selected.", nameof(transactionType));
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var purchaseCount = transactionType.Equals(ReJournalTypeAll, StringComparison.OrdinalIgnoreCase) ||
                                    transactionType.Equals(ReJournalTypePurchase, StringComparison.OrdinalIgnoreCase)
                    ? await ReJournalPurchaseAsync(month, year, cancellationToken)
                    : 0;

                var salesCount = transactionType.Equals(ReJournalTypeAll, StringComparison.OrdinalIgnoreCase) ||
                                 transactionType.Equals(ReJournalTypeSales, StringComparison.OrdinalIgnoreCase)
                    ? await ReJournalSalesAsync(month, year, cancellationToken)
                    : 0;

                var serviceCount = transactionType.Equals(ReJournalTypeAll, StringComparison.OrdinalIgnoreCase) ||
                                   transactionType.Equals(ReJournalTypeService, StringComparison.OrdinalIgnoreCase)
                    ? await ReJournalServiceAsync(month, year, userFullName, cancellationToken)
                    : 0;

                var collectionCount = transactionType.Equals(ReJournalTypeAll, StringComparison.OrdinalIgnoreCase) ||
                                      transactionType.Equals(ReJournalTypeCollection, StringComparison.OrdinalIgnoreCase)
                    ? await ReJournalCollectionAsync(month, year, cancellationToken)
                    : 0;

                var provisionalReceiptCount = transactionType.Equals(ReJournalTypeAll, StringComparison.OrdinalIgnoreCase) ||
                                              transactionType.Equals(ReJournalTypeProvisionalReceipt, StringComparison.OrdinalIgnoreCase)
                    ? await ReJournalProvisionalReceiptAsync(month, year, cancellationToken)
                    : 0;

                var debitMemoCount = transactionType.Equals(ReJournalTypeAll, StringComparison.OrdinalIgnoreCase) ||
                                     transactionType.Equals(ReJournalTypeDebitMemo, StringComparison.OrdinalIgnoreCase)
                    ? await ReJournalDebitMemoAsync(month, year, cancellationToken)
                    : 0;

                var creditMemoCount = transactionType.Equals(ReJournalTypeAll, StringComparison.OrdinalIgnoreCase) ||
                                      transactionType.Equals(ReJournalTypeCreditMemo, StringComparison.OrdinalIgnoreCase)
                    ? await ReJournalCreditMemoAsync(month, year, cancellationToken)
                    : 0;

                var paymentCount = transactionType.Equals(ReJournalTypeAll, StringComparison.OrdinalIgnoreCase) ||
                                   transactionType.Equals(ReJournalTypePayment, StringComparison.OrdinalIgnoreCase)
                    ? await ReJournalPaymentAsync(month, year, cancellationToken)
                    : 0;

                var jvCount = transactionType.Equals(ReJournalTypeAll, StringComparison.OrdinalIgnoreCase) ||
                              transactionType.Equals(ReJournalTypeJv, StringComparison.OrdinalIgnoreCase)
                    ? await ReJournalJvAsync(month, year, cancellationToken)
                    : 0;

                await transaction.CommitAsync(cancellationToken);

                return new ReJournalBatchResult
                {
                    PurchaseCount = purchaseCount,
                    SalesCount = salesCount,
                    ServiceCount = serviceCount,
                    CollectionCount = collectionCount,
                    ProvisionalReceiptCount = provisionalReceiptCount,
                    DebitMemoCount = debitMemoCount,
                    CreditMemoCount = creditMemoCount,
                    PaymentCount = paymentCount,
                    JvCount = jvCount
                };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<int> ReJournalPurchaseAsync(int month, int year, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var records = await unitOfWork.FilprideReceivingReport
                .GetAllQuery(x =>
                    x.Status == nameof(Status.Posted) &&
                    x.Date >= startDate &&
                    x.Date < endDate)
                .OrderBy(x => x.Date)
                .ToListAsync(cancellationToken);

            if (records.Count == 0)
            {
                return 0;
            }

            var references = records
                .Select(x => x.ReceivingReportNo!)
                .Distinct()
                .ToList();

            await dbContext.FilprideGeneralLedgerBooks
                .Where(x => references.Contains(x.Reference))
                .ExecuteDeleteAsync(cancellationToken);
            await dbContext.FilprideInventories
                .Where(x => references.Contains(x.Reference!))
                .ExecuteDeleteAsync(cancellationToken);

            var accountTitlesDto = await unitOfWork.FilprideReceivingReport
                .GetListOfAccountTitleDto(cancellationToken);
            foreach (var receivingReport in records)
            {
                await unitOfWork.FilprideInventory.AddPurchaseToInventoryAsync(receivingReport, cancellationToken);
                await unitOfWork.FilprideReceivingReport.PostAsync(
                    receivingReport,
                    cancellationToken,
                    accountTitlesDto);
            }

            return records.Count;
        }

        private async Task<int> ReJournalSalesAsync(int month, int year, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var records = await unitOfWork.FilprideDeliveryReceipt
                .GetAllQuery(x =>
                        x.VoidedBy == null &&
                        x.CanceledDate == null &&
                        x.DeliveredDate.HasValue &&
                        x.DeliveredDate.Value >= startDate &&
                        x.DeliveredDate.Value < endDate)
                .OrderBy(x => x.DeliveredDate)
                .ToListAsync(cancellationToken);

            if (records.Count == 0)
            {
                return 0;
            }

            var references = records
                .Select(x => x.DeliveryReceiptNo)
                .Distinct()
                .ToList();

            await dbContext.FilprideGeneralLedgerBooks
                .Where(x => references.Contains(x.Reference))
                .ExecuteDeleteAsync(cancellationToken);
            await dbContext.FilprideInventories
                .Where(x => references.Contains(x.Reference!))
                .ExecuteDeleteAsync(cancellationToken);

            var accountTitlesDto = await unitOfWork.FilprideDeliveryReceipt
                .GetListOfAccountTitleDto(cancellationToken);
            foreach (var dr in records)
            {
                await unitOfWork.FilprideInventory.AddSalesToInventoryAsync(dr, cancellationToken);
                await unitOfWork.FilprideDeliveryReceipt.PostAsync(dr, cancellationToken, accountTitlesDto);
            }

            return records.Count;
        }

        private async Task<int> ReJournalServiceAsync(int month, int year, string userFullName, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var records = await unitOfWork.FilprideServiceInvoice
                .GetAllQuery(x =>
                        x.Status == nameof(Status.Posted) &&
                        x.Period >= startDate &&
                        x.Period < endDate)
                .OrderBy(x => x.Period)
                .ToListAsync(cancellationToken);

            if (records.Count == 0)
            {
                return 0;
            }

            var references = records
                .Select(x => x.ServiceInvoiceNo)
                .Distinct()
                .ToList();

            await dbContext.FilprideGeneralLedgerBooks
                .Where(x => references.Contains(x.Reference))
                .ExecuteDeleteAsync(cancellationToken);

            foreach (var service in records.Where(x => x.ServiceName == "TRANSACTION FEE"))
            {
                await RevertTheReversalOfDrEntriesAsync(service.DeliveryReceiptId, cancellationToken);
            }

            var accountTitlesDto = await unitOfWork.FilprideServiceInvoice
                .GetListOfAccountTitleDto(cancellationToken);
            foreach (var service in records)
            {
                await unitOfWork.FilprideServiceInvoice.PostAsync(service, cancellationToken, accountTitlesDto);

                if (service.ServiceName == "TRANSACTION FEE")
                {
                    await ReverseDrEntriesAsync(service.DeliveryReceiptId, userFullName, cancellationToken);
                }
            }

            return records.Count;
        }

        private async Task<int> ReJournalPaymentAsync(int month, int year, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await unitOfWork.FilprideCheckVoucher
                .RebuildGeneralLedgerAsync(startDate, endDate, cancellationToken);
        }

        private async Task<int> ReJournalCollectionAsync(int month, int year, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var records = await unitOfWork.FilprideCollectionReceipt.GetAllQuery(x =>
                    x.PostedBy != null &&
                    x.Status != nameof(CollectionReceiptStatus.Voided) &&
                    x.Status != nameof(CollectionReceiptStatus.Canceled) &&
                    x.TransactionDate >= startDate &&
                    x.TransactionDate < endDate)
                .OrderBy(x => x.TransactionDate)
                .ToListAsync(cancellationToken);

            if (records.Count == 0)
            {
                return 0;
            }

            var references = records
                .Select(x => x.CollectionReceiptNo!)
                .Distinct()
                .ToList();

            await dbContext.FilprideGeneralLedgerBooks
                .Where(x => references.Contains(x.Reference))
                .ExecuteDeleteAsync(cancellationToken);

            var accountTitlesDto = await unitOfWork.FilprideCollectionReceipt
                .GetListOfAccountTitleDto(cancellationToken);
            var costOfMoneyReceipts = records
                .Where(record =>
                    IsSalesCollection(record) &&
                    record.DepositedDate.HasValue &&
                    record.ClearedDate.HasValue)
                .ToList();
            var affectedSalesInvoices = await GetCollectionSalesInvoicesAsync(costOfMoneyReceipts, cancellationToken);
            var affectedDeliveryReceiptIds = affectedSalesInvoices.Values
                .Where(invoice => invoice.DeliveryReceiptId.HasValue)
                .Select(invoice => invoice.DeliveryReceiptId!.Value)
                .Distinct()
                .ToList();

            foreach (var record in records)
            {
                await unitOfWork.FilprideCollectionReceipt.PostAsync(
                    record,
                    cancellationToken,
                    accountTitlesDto,
                    saveChanges: false,
                    postedDateAndTime: record.PostedDate ?? record.CreatedDate);

                if (record.DepositedDate != null && record.ClearedDate != null)
                {
                    await unitOfWork.FilprideCollectionReceipt.ApplyClearingDateAsync(
                        record,
                        cancellationToken,
                        accountTitlesDto,
                        saveChanges: false);
                }
            }

            await RebuildCollectionCostOfMoneyAsync(
                affectedDeliveryReceiptIds,
                accountTitlesDto,
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return records.Count;
        }

        private static bool IsSalesCollection(FilprideCollectionReceipt collectionReceipt)
        {
            return collectionReceipt.SalesInvoiceId.HasValue ||
                   collectionReceipt.MultipleSIId is { Length: > 0 };
        }

        private async Task<int> ReJournalProvisionalReceiptAsync(int month, int year, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await unitOfWork.ProvisionalReceipt
                .RebuildGeneralLedgerAsync(startDate, endDate, cancellationToken);
        }

        private async Task RebuildCollectionCostOfMoneyAsync(
            IReadOnlyCollection<int> deliveryReceiptIds,
            List<AccountTitleDto> accountTitlesDto,
            CancellationToken cancellationToken)
        {
            if (deliveryReceiptIds.Count == 0)
            {
                return;
            }

            var salesInvoices = await dbContext.FilprideSalesInvoices
                .Include(invoice => invoice.DeliveryReceipt)
                .ThenInclude(deliveryReceipt => deliveryReceipt!.Hauler)
                .Include(invoice => invoice.DeliveryReceipt)
                .ThenInclude(deliveryReceipt => deliveryReceipt!.Commissionee)
                .Include(invoice => invoice.DeliveryReceipt)
                .ThenInclude(deliveryReceipt => deliveryReceipt!.CustomerOrderSlip)
                .ThenInclude(customerOrderSlip => customerOrderSlip!.Product)
                .Where(invoice =>
                    invoice.DeliveryReceiptId.HasValue &&
                    deliveryReceiptIds.Contains(invoice.DeliveryReceiptId.Value))
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            var salesInvoicesByNumber = salesInvoices
                .Where(invoice => invoice.SalesInvoiceNo != null)
                .GroupBy(invoice => invoice.SalesInvoiceNo!)
                .ToDictionary(group => group.Key, group => group.First());
            var invoiceNumbers = salesInvoicesByNumber.Keys.ToList();

            var collectionDetails = await dbContext.FilprideCollectionReceiptDetails
                .AsNoTracking()
                .Include(detail => detail.FilprideCollectionReceipt)
                .Where(detail =>
                    invoiceNumbers.Contains(detail.InvoiceNo) &&
                    detail.FilprideCollectionReceipt!.PostedBy != null &&
                    detail.FilprideCollectionReceipt.Status != nameof(CollectionReceiptStatus.Voided) &&
                    detail.FilprideCollectionReceipt.Status != nameof(CollectionReceiptStatus.Canceled) &&
                    detail.FilprideCollectionReceipt.DepositedDate.HasValue &&
                    detail.FilprideCollectionReceipt.ClearedDate.HasValue)
                .OrderBy(detail => detail.FilprideCollectionReceipt!.DepositedDate)
                .ThenBy(detail => detail.CollectionReceiptId)
                .ThenBy(detail => detail.Id)
                .ToListAsync(cancellationToken);

            var deliveryReceipts = salesInvoices
                .Where(invoice => invoice.DeliveryReceipt != null)
                .Select(invoice => invoice.DeliveryReceipt!)
                .GroupBy(deliveryReceipt => deliveryReceipt.DeliveryReceiptId)
                .ToDictionary(group => group.Key, group => group.First());
            var deliveryReceiptReferences = deliveryReceipts.Values
                .Select(deliveryReceipt => deliveryReceipt.DeliveryReceiptNo)
                .Distinct()
                .ToList();

            await dbContext.FilprideGeneralLedgerBooks
                .Where(entry =>
                    deliveryReceiptReferences.Contains(entry.Reference) &&
                    entry.Description.StartsWith("Cost of money from late deposit"))
                .ExecuteDeleteAsync(cancellationToken);

            var eligibleDeliveryReceiptIds = new HashSet<int>();
            foreach (var deliveryReceipt in deliveryReceipts.Values)
            {
                deliveryReceipt.CommissionAmount = DecimalRoundingHelper.ComputeAmountFromUnitPrice(
                    deliveryReceipt.Quantity,
                    deliveryReceipt.CommissionRate);

                if (deliveryReceipt.CommissionAmount > 0)
                {
                    eligibleDeliveryReceiptIds.Add(deliveryReceipt.DeliveryReceiptId);
                }
            }

            var costRanges = collectionDetails
                .Where(detail => salesInvoicesByNumber.ContainsKey(detail.InvoiceNo))
                .Select(detail => new
                {
                    salesInvoicesByNumber[detail.InvoiceNo].DueDate,
                    DepositedDate = detail.FilprideCollectionReceipt!.DepositedDate!.Value
                })
                .Where(range => range.DueDate <= range.DepositedDate)
                .ToList();
            HashSet<DateOnly> nonWorkingDays = costRanges.Count == 0
                ? []
                : (await DateTimeHelper.GetNonWorkingDays(
                    costRanges.Min(range => range.DueDate),
                    costRanges.Max(range => range.DepositedDate))).ToHashSet();

            foreach (var detail in collectionDetails)
            {
                if (!salesInvoicesByNumber.TryGetValue(detail.InvoiceNo, out var salesInvoice) ||
                    !salesInvoice.DeliveryReceiptId.HasValue ||
                    !deliveryReceipts.TryGetValue(salesInvoice.DeliveryReceiptId.Value, out var deliveryReceipt) ||
                    deliveryReceipt.CustomerOrderSlip == null)
                {
                    continue;
                }

                var depositedDate = detail.FilprideCollectionReceipt!.DepositedDate!.Value;
                var nonWorkingDayCount = nonWorkingDays.Count(day =>
                    day >= salesInvoice.DueDate &&
                    day <= depositedDate);
                var daysDelayed = depositedDate.DayNumber -
                                  salesInvoice.DueDate.DayNumber -
                                  nonWorkingDayCount;

                if (daysDelayed <= 0 ||
                    !eligibleDeliveryReceiptIds.Contains(deliveryReceipt.DeliveryReceiptId))
                {
                    continue;
                }

                var paymentAmount = detail.Amount - detail.EWT - detail.WVAT;
                if (paymentAmount <= 0)
                {
                    continue;
                }

                var costOfMoney = paymentAmount * .03m * daysDelayed / 360m;

                await unitOfWork.FilprideCollectionReceipt.ApplyCostOfMoney(deliveryReceipt, costOfMoney,
                    "SYSTEM GENERATED",
                    depositedDate,
                    cancellationToken,
                    accountTitlesDto,
                    saveChanges: false,
                    checkExistingEntry: false,
                    sourceCollectionReceiptDetailId: detail.Id);
            }
        }

        private async Task<Dictionary<string, FilprideSalesInvoice>> GetCollectionSalesInvoicesAsync(
            IEnumerable<FilprideCollectionReceipt> collectionReceipts,
            CancellationToken cancellationToken)
        {
            var invoiceNumbers = collectionReceipts
                .SelectMany(receipt => receipt.ReceiptDetails ?? [])
                .Select(detail => detail.InvoiceNo)
                .Distinct()
                .ToList();

            if (invoiceNumbers.Count == 0)
            {
                return [];
            }

            var salesInvoices = await dbContext.FilprideSalesInvoices
                .Include(invoice => invoice.Product)
                .Include(invoice => invoice.Customer)
                .Include(invoice => invoice.DeliveryReceipt)
                .ThenInclude(deliveryReceipt => deliveryReceipt!.PurchaseOrder)
                .Include(invoice => invoice.DeliveryReceipt)
                .ThenInclude(deliveryReceipt => deliveryReceipt!.Hauler)
                .Include(invoice => invoice.DeliveryReceipt)
                .ThenInclude(deliveryReceipt => deliveryReceipt!.Commissionee)
                .Include(invoice => invoice.DeliveryReceipt)
                .ThenInclude(deliveryReceipt => deliveryReceipt!.CustomerOrderSlip)
                .ThenInclude(customerOrderSlip => customerOrderSlip!.Product)
                .Include(invoice => invoice.CustomerOrderSlip)
                .ThenInclude(customerOrderSlip => customerOrderSlip!.Product)
                .Where(invoice => invoiceNumbers.Contains(invoice.SalesInvoiceNo!))
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            return salesInvoices
                .GroupBy(invoice => invoice.SalesInvoiceNo!)
                .ToDictionary(group => group.Key, group => group.First());
        }

        private async Task<int> ReJournalDebitMemoAsync(int month, int year, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var records = await unitOfWork.FilprideDebitMemo.GetAllQuery(x =>
                    x.PostedBy != null &&
                    x.Status == nameof(Status.Posted) &&
                    x.TransactionDate >= startDate &&
                    x.TransactionDate < endDate)
                .OrderBy(x => x.TransactionDate)
                .ToListAsync(cancellationToken);

            if (records.Count == 0)
            {
                return 0;
            }

            var references = records
                .Select(x => x.DebitMemoNo!)
                .Distinct()
                .ToList();

            await dbContext.FilprideGeneralLedgerBooks
                .Where(x => references.Contains(x.Reference))
                .ExecuteDeleteAsync(cancellationToken);

            var accountTitlesDto = await unitOfWork.FilprideDebitMemo
                .GetListOfAccountTitleDto(cancellationToken);
            foreach (var record in records)
            {
                await unitOfWork.FilprideDebitMemo.PostAsync(
                    record,
                    cancellationToken,
                    accountTitlesDto,
                    saveChanges: false);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return records.Count;
        }

        private async Task<int> ReJournalCreditMemoAsync(int month, int year, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var records = await unitOfWork.FilprideCreditMemo.GetAllQuery(x =>
                    x.PostedBy != null &&
                    x.Status == nameof(Status.Posted) &&
                    x.TransactionDate >= startDate &&
                    x.TransactionDate < endDate)
                .OrderBy(x => x.TransactionDate)
                .ToListAsync(cancellationToken);

            if (records.Count == 0)
            {
                return 0;
            }

            var references = records
                .Select(x => x.CreditMemoNo!)
                .Distinct()
                .ToList();

            await dbContext.FilprideGeneralLedgerBooks
                .Where(x => references.Contains(x.Reference))
                .ExecuteDeleteAsync(cancellationToken);

            var accountTitlesDto = await unitOfWork.FilprideCreditMemo
                .GetListOfAccountTitleDto(cancellationToken);
            foreach (var record in records)
            {
                await unitOfWork.FilprideCreditMemo.PostAsync(
                    record,
                    cancellationToken,
                    accountTitlesDto,
                    saveChanges: false);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return records.Count;
        }

        private async Task<int> ReJournalJvAsync(int month, int year, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var jvs = await dbContext.FilprideJournalVoucherHeaders
                .AsNoTracking()
                .Include(x => x.Details)
                .Include(x => x.CheckVoucherHeader)
                .Where(x =>

                    x.PostedBy != null &&
                    x.Date >= startDate &&
                    x.Date < endDate)
                .ToListAsync(cancellationToken);

            if (jvs.Count == 0)
            {
                return 0;
            }

            var references = jvs
                .Select(x => x.JournalVoucherHeaderNo!)
                .Distinct()
                .ToList();

            await dbContext.FilprideGeneralLedgerBooks
                .Where(x => references.Contains(x.Reference))
                .ExecuteDeleteAsync(cancellationToken);

            var accountTitlesDto = await unitOfWork.FilprideJournalVoucher
                .GetListOfAccountTitleDto(cancellationToken);
            foreach (var jv in jvs.OrderBy(x => x.Date))
            {
                await unitOfWork.FilprideJournalVoucher.PostAsync(
                    jv,
                    jv.Details!,
                    cancellationToken,
                    accountTitlesDto,
                    saveChanges: false);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return jvs.Count;
        }

        private async Task RevertTheReversalOfDrEntriesAsync(int? deliveryReceiptId, CancellationToken cancellationToken)
        {
            if (!deliveryReceiptId.HasValue)
            {
                return;
            }

            var dr = await unitOfWork.FilprideDeliveryReceipt
                .GetAsync(x => x.DeliveryReceiptId == deliveryReceiptId.Value, cancellationToken);

            if (dr == null)
            {
                return;
            }

            var relatedRrNo = (await unitOfWork.FilprideReceivingReport
                    .GetAsync(x => x.DeliveryReceiptId == dr.DeliveryReceiptId, cancellationToken))?
                .ReceivingReportNo;

            await dbContext.FilprideGeneralLedgerBooks
                .Where(x => (x.Reference == dr.DeliveryReceiptNo || (relatedRrNo != null && x.Reference == relatedRrNo))
                            && x.Description.Contains("Reversal of entries due to recording of transaction fee."))
                .ExecuteDeleteAsync(cancellationToken);
        }

        private async Task ReverseDrEntriesAsync(int? deliveryReceiptId, string userFullName, CancellationToken cancellationToken)
        {
            if (!deliveryReceiptId.HasValue)
            {
                return;
            }

            var dr = await unitOfWork.FilprideDeliveryReceipt
                .GetAsync(x => x.DeliveryReceiptId == deliveryReceiptId.Value, cancellationToken);

            if (dr == null)
            {
                return;
            }

            var relatedRrNo = (await unitOfWork.FilprideReceivingReport
                    .GetAsync(x => x.DeliveryReceiptId == dr.DeliveryReceiptId, cancellationToken))?
                .ReceivingReportNo;

            var originalEntries = await dbContext.FilprideGeneralLedgerBooks
                .Where(x => (x.Reference == dr.DeliveryReceiptNo || (relatedRrNo != null && x.Reference == relatedRrNo))
)
                .ToListAsync(cancellationToken);

            var reversalEntries = new List<FilprideGeneralLedgerBook>();

            foreach (var originalEntry in originalEntries)
            {
                reversalEntries.Add(new FilprideGeneralLedgerBook
                {
                    Date = new DateOnly(
                        originalEntry.Date.Year,
                        originalEntry.Date.Month,
                        DateTime.DaysInMonth(originalEntry.Date.Year, originalEntry.Date.Month)),
                    Reference = originalEntry.Reference,
                    AccountNo = originalEntry.AccountNo,
                    AccountTitle = originalEntry.AccountTitle,
                    Description = "Reversal of entries due to recording of transaction fee.",
                    Debit = originalEntry.Credit,
                    Credit = originalEntry.Debit,
                    CreatedBy = userFullName,
                    CreatedDate = DateTimeHelper.GetCurrentPhilippineTime(),
                    IsPosted = true,
                    AccountId = originalEntry.AccountId,
                    SubAccountType = originalEntry.SubAccountType,
                    SubAccountId = originalEntry.SubAccountId,
                    SubAccountName = originalEntry.SubAccountName,
                    CounterpartyType = originalEntry.CounterpartyType,
                    CounterpartyId = originalEntry.CounterpartyId,
                    CounterpartyName = originalEntry.CounterpartyName,
                    ModuleType = originalEntry.ModuleType,
                });
            }

            await dbContext.FilprideGeneralLedgerBooks.AddRangeAsync(reversalEntries, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task UpdateGeneralLedgerBooksAsync(string referenceNo, string particulars, CancellationToken cancellationToken)
        {
            await dbContext.FilprideGeneralLedgerBooks
                .Where(x => x.Reference == referenceNo)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Description, particulars),
                    cancellationToken);
        }
    }
}
