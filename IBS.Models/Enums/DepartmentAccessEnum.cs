namespace IBS.Models.Enums
{
    public enum AccessModule
    {
        CustomerOrderSlip,
        DeliveryReceipts,
        SalesInvoice,
        ServiceInvoice,
        CollectionReceipt,
        DebitMemo,
        CreditMemo,
        AuthorityToLoad,
        PurchaseOrder,
        ReceivingReport,
        CheckVoucherTrade,
        CheckVoucherNonTradeInvoice,
        CheckVoucherNonTradePayrollInvoice,
        CheckVoucherNonTradePayment,
        JournalVoucher,
        Disbursement,
        ProvisionalReceipt
    }

    #region Module Actions

    public enum CustomerOrderSlip
    {
        CustomerOrderSlipPreview,
        CustomerOrderSlipEdit,
        CustomerOrderSlipClose,
        CustomerOrderSlipAppointSupplier,
        CustomerOrderSlipReAppointSupplier,
        CustomerOrderSlipChangePrice,
        CustomerOrderSlipChangeCommission,
        CustomerOrderSlipCreate
    }

    public enum DeliveryReceipts
    {
        DeliveryReceiptsCancel,
        DeliveryReceiptsChangeHaulerFreight,
        DeliveryReceiptsCreate,
        DeliveryReceiptsEdit,
        DeliveryReceiptsPreview,
        DeliveryReceiptsRecordLiftingDate,
        DeliveryReceiptsMarkAsDelivered
    }

    public enum SalesInvoice
    {
        SalesInvoiceCancel,
        SalesInvoiceCreate,
        SalesInvoiceEdit,
        SalesInvoicePost,
        SalesInvoicePreview,
        SalesInvoiceUnpost
    }

    public enum ServiceInvoice
    {
        ServiceInvoiceCancel,
        ServiceInvoiceCreate,
        ServiceInvoiceEdit,
        ServiceInvoicePost,
        ServiceInvoicePreview,
        ServiceInvoiceUnpost
    }

    public enum CollectionReceipt
    {
        CollectionReceiptApplyClearingDate,
        CollectionReceiptCancel,
        CollectionReceiptCreateForService,
        CollectionReceiptAddDepositInfo,
        CollectionReceiptEditForService,
        CollectionReceiptSingleCreateForSales,
        CollectionReceiptEditForSales,
        CollectionReceiptMultipleCollectionCreateForSales,
        CollectionReceiptMultipleCollectionEditForSales,
        CollectionReceiptMultipleCollectionPreview,
        CollectionReceiptPreview,
        CollectionReceiptPost,
        CollectionReceiptRedeposit,
        CollectionReceiptReturnCheck,
        CollectionReceiptUnpost
    }

    public enum DebitMemo
    {
        DebitMemoCancel,
        DebitMemoCreate,
        DebitMemoEdit,
        DebitMemoPost,
        DebitMemoPreview,
        DebitMemoUnpost
    }

    public enum CreditMemo
    {
        CreditMemoCancel,
        CreditMemoCreate,
        CreditMemoEdit,
        CreditMemoPost,
        CreditMemoPreview,
        CreditMemoUnpost
    }

    public enum AuthorityToLoad
    {
        AuthorityToLoadCreate,
        AuthorityToLoadEdit,
        AuthorityToLoadPreview,
        AuthorityToLoadUpdateValidUntil
    }

    public enum PurchaseOrder
    {
        PurchaseOrderCancel,
        PurchaseOrderClose,
        PurchaseOrderCreate,
        PurchaseOrderEdit,
        PurchaseOrderPost,
        PurchaseOrderPreview,
        PurchaseOrderProductTransfer,
        PurchaseOrderUpdatePrice,
        PurchaseOrderUpdateSupplierSalesOrderNo
    }

    public enum ReceivingReport
    {
        ReceivingReportCancel,
        ReceivingReportCreate,
        ReceivingReportEdit,
        ReceivingReportPost,
        ReceivingReportPreview
    }

    public enum CheckVoucherTrade
    {
        CheckVoucherTradeCancel,
        CheckVoucherTradeCreate,
        CheckVoucherTradeCreateCommissionPayment,
        CheckVoucherTradeCreateHaulerPayment,
        CheckVoucherTradeEdit,
        CheckVoucherTradeEditCommissionPayment,
        CheckVoucherTradeEditHaulerPayment,
        CheckVoucherTradePost,
        CheckVoucherTradePreview,
        CheckVoucherTradeUnpost
    }

    public enum CheckVoucherNonTradeInvoice
    {
        CheckVoucherNonTradeInvoiceCancel,
        CheckVoucherNonTradeInvoiceCreate,
        CheckVoucherNonTradeInvoiceEdit,
        CheckVoucherNonTradeInvoicePost,
        CheckVoucherNonTradeInvoicePreview,
        CheckVoucherNonTradeInvoiceUnpost
    }

    public enum CheckVoucherNonTradePayrollInvoice
    {
        CheckVoucherNonTradePayrollInvoiceCancel,
        CheckVoucherNonTradePayrollInvoiceCreate,
        CheckVoucherNonTradePayrollInvoiceEdit,
        CheckVoucherNonTradePayrollInvoicePost,
        CheckVoucherNonTradePayrollInvoicePreview,
        CheckVoucherNonTradePayrollInvoiceUnpost
    }

    public enum CheckVoucherNonTradePayment
    {
        CheckVoucherNonTradePaymentCancel,
        CheckVoucherNonTradePaymentCreate,
        CheckVoucherNonTradePaymentCreateAdvancesToEmployee,
        CheckVoucherNonTradePaymentCreateAdvancesToSupplier,
        CheckVoucherNonTradePaymentEdit,
        CheckVoucherNonTradePaymentEditAdvancesToEmployee,
        CheckVoucherNonTradePaymentEditAdvancesToSupplier,
        CheckVoucherNonTradePaymentPost,
        CheckVoucherNonTradePaymentPreview,
        CheckVoucherNonTradePaymentUnpost,
        CheckVoucherNonTradePaymentLiquidateAdvances
    }

    public enum JournalVoucher
    {
        JournalVoucherCancel,
        JournalVoucherCreateAccrual,
        JournalVoucherCreateAmortization,
        JournalVoucherCreateLiquidation,
        JournalVoucherCreateReclass,
        JournalVoucherEditAccrual,
        JournalVoucherEditAmortization,
        JournalVoucherEditLiquidation,
        JournalVoucherEditReclass,
        JournalVoucherPost,
        JournalVoucherPreview,
        JournalVoucherUnpost
    }

    public enum Disbursement
    {
        DisbursementUpdateDcpDate,
        DisbursementUpdateDcrDate
    }

    public enum ProvisionalReceipt
    {
        ProvisionalReceiptApplyClearingDate,
        ProvisionalReceiptCancel,
        ProvisionalReceiptCreate,
        ProvisionalReceiptAddDepositInfo,
        ProvisionalReceiptEdit,
        ProvisionalReceiptPost,
        ProvisionalReceiptPreview,
        ProvisionalReceiptRedeposit,
        ProvisionalReceiptReturnCheck,
        ProvisionalReceiptUnpost
    }

    #endregion
}
