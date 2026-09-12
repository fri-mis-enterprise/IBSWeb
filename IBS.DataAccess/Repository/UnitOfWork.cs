using System.ComponentModel;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.DataAccess.Repository.Filpride;
using IBS.DataAccess.Repository.IRepository;
using IBS.DataAccess.Repository.MasterFile.IRepository;
using IBS.DataAccess.Repository.MasterFile;
using IBS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IProductRepository = IBS.DataAccess.Repository.MasterFile.IRepository.IProductRepository;
using ProductRepository = IBS.DataAccess.Repository.MasterFile.ProductRepository;

namespace IBS.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IDepartmentAccessRepository DepartmentAccess { get; }

        public INotificationRepository Notifications { get; private set; }

        public async Task<bool> IsPeriodPostedAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            return await _db.PostedPeriods
                .AnyAsync(m => m.IsPosted
                               && m.Month == date.Month
                               && m.Year == date.Year, cancellationToken);
        }

        public async Task<DateTime> GetMinimumPeriodBasedOnThePostedPeriods(Module module, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(Module), module))
            {
                throw new InvalidEnumArgumentException(nameof(module), (int)module, typeof(Module));
            }

            var period = await _db.PostedPeriods
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .FirstOrDefaultAsync(x => x.Module == module.ToString()
                                          && x.IsPosted, cancellationToken);

            if (period == null)
            {
                return DateTime.MinValue;
            }

            return new DateOnly(period.Year, period.Month, 1)
                .AddMonths(1)
                .ToDateTime(new TimeOnly(0, 0));
        }

        public async Task<bool> IsPeriodPostedAsync(Module module, DateOnly date, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(Module), module))
            {
                throw new InvalidEnumArgumentException(nameof(module), (int)module, typeof(Module));
            }

            return await _db.PostedPeriods
                .AnyAsync(m =>
                    m.Module == module.ToString() &&
                    m.IsPosted &&
                    m.Year == date.Year &&
                    m.Month == date.Month,
                    cancellationToken);
        }

        #region--Filpride

        public ICustomerOrderSlipRepository FilprideCustomerOrderSlip { get; private set; }
        public IDeliveryReceiptRepository FilprideDeliveryReceipt { get; private set; }
        public ICustomerRepository FilprideCustomer { get; private set; }
        public ISupplierRepository FilprideSupplier { get; private set; }
        public IPickUpPointRepository FilpridePickUpPoint { get; private set; }
        public IAuthorityToLoadRepository FilprideAuthorityToLoad { get; private set; }
        public IChartOfAccountRepository FilprideChartOfAccount { get; private set; }
        public IAuditTrailRepository FilprideAuditTrail { get; private set; }
        public ICustomerBranchRepository FilprideCustomerBranch { get; private set; }
        public ITermsRepository FilprideTerms { get; private set; }
        public IGeneralLedgerRepository GeneralLedger { get; private set; }
        public IProvisionalReceiptRepository ProvisionalReceipt { get; private set; }
        public ILockedPeriodAdjustmentRepository LockedPeriodAdjustment { get; private set; }

        #endregion

        #region AAS

        #region Accounts Receivable
        public ISalesInvoiceRepository FilprideSalesInvoice { get; private set; }

        public IServiceInvoiceRepository FilprideServiceInvoice { get; private set; }

        public ICollectionReceiptRepository FilprideCollectionReceipt { get; private set; }

        public IDebitMemoRepository FilprideDebitMemo { get; private set; }

        public ICreditMemoRepository FilprideCreditMemo { get; private set; }
        #endregion

        #region Accounts Payable
        public ICheckVoucherRepository FilprideCheckVoucher { get; private set; }

        public IJournalVoucherRepository FilprideJournalVoucher { get; private set; }

        public IPurchaseOrderRepository FilpridePurchaseOrder { get; private set; }

        public IReceivingReportRepository FilprideReceivingReport { get; private set; }
        #endregion

        #region Books and Report
        public IInventoryRepository FilprideInventory { get; private set; }

        public IReportRepository FilprideReport { get; private set; }
        #endregion

        #region Master File

        public IBankAccountRepository FilprideBankAccount { get; private set; }

        public IServiceRepository FilprideService { get; private set; }

        #endregion

        #endregion

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;

            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            Notifications = new NotificationRepository(_db);
            DepartmentAccess = new DepartmentAccessRepository(_db);

            #region--Filpride

            FilprideCustomerOrderSlip = new CustomerOrderSlipRepository(_db);
            FilprideDeliveryReceipt = new DeliveryReceiptRepository(_db);
            FilprideCustomer = new CustomerRepository(_db);
            FilprideSupplier = new SupplierRepository(_db);
            FilpridePickUpPoint = new PickUpPointRepository(_db);
            FilprideAuthorityToLoad = new AuthorityToLoadRepository(_db);
            FilprideChartOfAccount = new ChartOfAccountRepository(_db);
            FilprideAuditTrail = new AuditTrailRepository(_db);
            FilprideCustomerBranch = new CustomerBranchRepository(_db);
            FilprideTerms = new TermsRepository(_db);
            GeneralLedger = new GeneralLedgerRepository(_db);
            ProvisionalReceipt = new ProvisionalReceiptRepository(_db);
            LockedPeriodAdjustment = new LockedPeriodAdjustmentRepository(_db);

            #endregion

            #region AAS

            #region Accounts Receivable
            FilprideSalesInvoice = new SalesInvoiceRepository(_db);
            FilprideServiceInvoice = new ServiceInvoiceRepository(_db);
            FilprideCollectionReceipt = new CollectionReceiptRepository(_db);
            FilprideDebitMemo = new DebitMemoRepository(_db);
            FilprideCreditMemo = new CreditMemoRepository(_db);
            #endregion

            #region Accounts Payable
            FilprideCheckVoucher = new CheckVoucherRepository(_db);
            FilprideJournalVoucher = new JournalVoucherRepository(_db);
            FilpridePurchaseOrder = new PurchaseOrderRepository(_db);
            FilprideReceivingReport = new ReceivingReportRepository(_db);
            #endregion

            #region Books and Report
            FilprideInventory = new InventoryRepository(_db);
            FilprideReport = new ReportRepository(_db);
            #endregion

            #region Master File

            FilprideBankAccount = new BankAccountRepository(_db);
            FilprideService = new ServiceRepository(_db);

            #endregion

            #endregion
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        public void Dispose() => _db.Dispose();

        #region--Filpride

        public async Task<List<SelectListItem>> GetFilprideCustomerListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideCustomers
                .OrderBy(c => c.CustomerName)
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.CustomerName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetFilprideSupplierListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideSuppliers
                .OrderBy(s => s.SupplierCode)
                .Where(s => s.IsActive)
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierCode + " " + s.SupplierName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetFilprideEmployeeSupplierListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideSuppliers
                .Where(s => s.IsActive && s.Category == "Employee")
                .OrderBy(s => s.EmployeeNumber)
                .ThenBy(s => s.SupplierName)
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = string.IsNullOrWhiteSpace(s.EmployeeNumber)
                        ? s.SupplierName
                        : $"{s.EmployeeNumber} - {s.SupplierName}"
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetFilprideTradeSupplierListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideSuppliers
                .OrderBy(s => s.SupplierCode)
                .Where(s => s.IsActive && s.Category == "Trade")
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierCode + " " + s.SupplierName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetFilprideNonTradeSupplierListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideSuppliers
                .OrderBy(s => s.SupplierName)
                .Where(s => s.IsActive && (s.Category == "Non-Trade" || s.Category == "Employee"))
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetFilprideCommissioneeListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideSuppliers
                .OrderBy(s => s.SupplierCode)
                .Where(s => s.IsActive && s.Category == "Commissionee")
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierCode + " " + s.SupplierName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetFilprideHaulerListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideSuppliers
                .OrderBy(s => s.SupplierCode)
                .Where(s => s.IsActive && s.Category == "Hauler")
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierCode + " " + s.SupplierName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetFilprideBankAccountListById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideBankAccounts
                .OrderBy(b => b.AccountNo)
                .Where(ba => ba.IsActive)
                .Select(ba => new SelectListItem
                {
                    Value = ba.BankAccountId.ToString(),
                    Text = ba.Bank + " " + ba.AccountNo + " " + ba.AccountName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetDistinctFilpridePickupPointListById(CancellationToken cancellationToken = default)
        {
            return await _db.FilpridePickUpPoints
                .GroupBy(p => p.Depot)
                .OrderBy(g => g.Key)
                .Select(g => new SelectListItem
                {
                    Value = g.First().PickUpPointId.ToString(),
                    Text = g.Key // g.Key is the Depot name
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetFilprideServiceListById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideServices
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = s.Name
                })
                .ToListAsync(cancellationToken);
        }

        #endregion

        public async Task<List<SelectListItem>> GetProductListAsyncByCode(CancellationToken cancellationToken = default)
        {
            return await _db.Products
                .OrderBy(p => p.ProductCode)
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductCode,
                    Text = p.ProductCode + " " + p.ProductName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetProductListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.Products
                .OrderBy(p => p.ProductCode)
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.ProductCode + " " + p.ProductName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetChartOfAccountListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideChartOfAccounts
                .Where(coa => !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountId.ToString(),
                    Text = s.AccountNumber + " " + s.AccountName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetChartOfAccountListAsyncByNo(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideChartOfAccounts
                .Where(coa => !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber,
                    Text = $"({s.AccountType}) {s.AccountNumber} {s.AccountName}"
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetChartOfAccountListAsyncByAccountTitle(CancellationToken cancellationToken = default)
        {
            return await _db.FilprideChartOfAccounts
                .Where(coa => !coa.HasChildren)
                .OrderBy(coa => coa.AccountNumber)
                .Select(s => new SelectListItem
                {
                    Value = s.AccountNumber + " " + s.AccountName,
                    Text = $"({s.AccountType}) {s.AccountNumber} {s.AccountName}"
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetCompanyListAsyncByName(CancellationToken cancellationToken = default)
        {
            return await _db.Companies
                .OrderBy(c => c.CompanyCode)
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CompanyName,
                    Text = c.CompanyCode + " " + c.CompanyName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SelectListItem>> GetCompanyListAsyncById(CancellationToken cancellationToken = default)
        {
            return await _db.Companies
                .OrderBy(c => c.CompanyCode)
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CompanyId.ToString(),
                    Text = c.CompanyCode + " " + c.CompanyName
                })
                .ToListAsync(cancellationToken);
        }
    }
}
