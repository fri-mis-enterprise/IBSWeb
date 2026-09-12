using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.DataAccess.Repository.MasterFile.IRepository;
using IBS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Product { get; }

        ICompanyRepository Company { get; }

        Task SaveAsync(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetProductListAsyncByCode(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetProductListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetChartOfAccountListAsyncByNo(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetChartOfAccountListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetChartOfAccountListAsyncByAccountTitle(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetCompanyListAsyncByName(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetCompanyListAsyncById(CancellationToken cancellationToken = default);

        #region--Filpride

        IChartOfAccountRepository FilprideChartOfAccount { get; }
        ICustomerOrderSlipRepository FilprideCustomerOrderSlip { get; }
        IDeliveryReceiptRepository FilprideDeliveryReceipt { get; }
        ISupplierRepository FilprideSupplier { get; }
        ICustomerRepository FilprideCustomer { get; }
        IAuditTrailRepository FilprideAuditTrail { get; }
        ICustomerBranchRepository FilprideCustomerBranch { get; }
        ITermsRepository FilprideTerms { get; }
        IGeneralLedgerRepository GeneralLedger { get; }
        IProvisionalReceiptRepository ProvisionalReceipt { get; }
        ILockedPeriodAdjustmentRepository LockedPeriodAdjustment { get; }
        IDepartmentAccessRepository DepartmentAccess { get; }

        Task<List<SelectListItem>> GetFilprideCustomerListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetFilprideSupplierListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetFilprideEmployeeSupplierListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetFilprideTradeSupplierListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetFilprideNonTradeSupplierListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetFilprideCommissioneeListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetFilprideHaulerListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetFilprideBankAccountListById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetDistinctFilpridePickupPointListById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetFilprideServiceListById(CancellationToken cancellationToken = default);

        #endregion

        #region AAS

        #region Accounts Receivable
        ISalesInvoiceRepository FilprideSalesInvoice { get; }

        IServiceInvoiceRepository FilprideServiceInvoice { get; }

        ICollectionReceiptRepository FilprideCollectionReceipt { get; }

        IDebitMemoRepository FilprideDebitMemo { get; }

        ICreditMemoRepository FilprideCreditMemo { get; }
        #endregion

        #region Accounts Payable

        ICheckVoucherRepository FilprideCheckVoucher { get; }

        IJournalVoucherRepository FilprideJournalVoucher { get; }

        IPurchaseOrderRepository FilpridePurchaseOrder { get; }

        IReceivingReportRepository FilprideReceivingReport { get; }

        #endregion

        #region Books and Report
        IInventoryRepository FilprideInventory { get; }

        IReportRepository FilprideReport { get; }
        #endregion

        #region Master File

        IBankAccountRepository FilprideBankAccount { get; }

        IServiceRepository FilprideService { get; }

        IPickUpPointRepository FilpridePickUpPoint { get; }

        IAuthorityToLoadRepository FilprideAuthorityToLoad { get; }

        #endregion

        #endregion

        INotificationRepository Notifications { get; }

        Task<bool> IsPeriodPostedAsync(DateOnly date, CancellationToken cancellationToken = default);

        Task<DateTime> GetMinimumPeriodBasedOnThePostedPeriods(Module module, CancellationToken cancellationToken = default);

        Task<bool> IsPeriodPostedAsync(Module module, DateOnly date, CancellationToken cancellationToken = default);
    }
}
