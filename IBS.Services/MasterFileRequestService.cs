using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.Data;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Enums;
using IBS.Models.Filpride.Books;
using IBS.Models.Filpride.MasterFile;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace IBS.Services
{
    public sealed class MasterFileRequestService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly ApplicationDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public MasterFileRequestService(
            ApplicationDbContext dbContext,
            IUnitOfWork unitOfWork,
            ICacheService cacheService)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public IQueryable<FilprideMasterFileRequest> GetRequests() =>
            _dbContext.FilprideMasterFileRequests.AsNoTracking();

        public async Task<FilprideMasterFileRequest?> GetAsync(int id, CancellationToken cancellationToken = default) =>
            await _dbContext.FilprideMasterFileRequests.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        public object DeserializeModel(FilprideMasterFileRequest request) => request.MasterFileType switch
        {
            FilprideMasterFileType.Customer => ToModel(Deserialize<CustomerRequestPayload>(request)),
            FilprideMasterFileType.CustomerBranch => ToModel(Deserialize<CustomerBranchRequestPayload>(request)),
            FilprideMasterFileType.Supplier => ToModel(Deserialize<SupplierRequestPayload>(request)),
            FilprideMasterFileType.BankAccount => ToModel(Deserialize<BankAccountRequestPayload>(request)),
            FilprideMasterFileType.Service => ToModel(Deserialize<ServiceRequestPayload>(request)),
            FilprideMasterFileType.ChartOfAccount => Deserialize<ChartOfAccountRequestPayload>(request),
            FilprideMasterFileType.PickupPoint => ToModel(Deserialize<PickupPointRequestPayload>(request)),
            _ => throw new InvalidOperationException("Unsupported master-file request type.")
        };

        public async Task<int> SaveRequestAsync(
            FilprideMasterFileType type,
            object model,
            string requestedBy,
            string requestedByName,
            int? requestId = null,
            CancellationToken cancellationToken = default)
        {
            ValidateSubmission(type, model);
            await ValidateReferencesAsync(type, model, cancellationToken);
            string payloadJson = SerializePayload(type, model);
            DateTime now = DateTimeHelper.GetCurrentPhilippineTime();

            if (requestId == null)
            {
                var request = new FilprideMasterFileRequest
                {
                    MasterFileType = type,
                    PayloadJson = payloadJson,
                    RequestedBy = requestedBy,
                    RequestedByName = requestedByName,
                    RequestedDate = now,
                    LastModifiedDate = now
                };

                _dbContext.FilprideMasterFileRequests.Add(request);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return request.Id;
            }

            var existing = await _dbContext.FilprideMasterFileRequests
                .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
                ?? throw new InvalidOperationException("Request not found.");

            if (existing.RequestedBy != requestedBy)
            {
                throw new UnauthorizedAccessException("You can only edit your own requests.");
            }

            if (existing.MasterFileType != type)
            {
                throw new InvalidOperationException("The master-file type cannot be changed.");
            }

            if (existing.Status is not (FilprideMasterFileRequestStatus.ForApproval or FilprideMasterFileRequestStatus.Rejected))
            {
                throw new InvalidOperationException("This request can no longer be edited.");
            }

            existing.PayloadJson = payloadJson;
            existing.Status = FilprideMasterFileRequestStatus.ForApproval;
            existing.LastModifiedDate = now;
            existing.ApprovedBy = null;
            existing.ApprovedDate = null;
            existing.Remarks = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        public async Task CancelAsync(int id, string requestedBy, CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.FilprideMasterFileRequests
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
                ?? throw new InvalidOperationException("Request not found.");

            if (request.RequestedBy != requestedBy)
            {
                throw new UnauthorizedAccessException("You can only cancel your own requests.");
            }

            if (request.Status is not (FilprideMasterFileRequestStatus.ForApproval or FilprideMasterFileRequestStatus.Rejected))
            {
                throw new InvalidOperationException("This request can no longer be canceled.");
            }

            request.Status = FilprideMasterFileRequestStatus.Canceled;
            request.LastModifiedDate = DateTimeHelper.GetCurrentPhilippineTime();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RejectAsync(int id, string approver, string remarks, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                throw new InvalidOperationException("Remarks are required when rejecting a request.");
            }

            var request = await _dbContext.FilprideMasterFileRequests
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
                ?? throw new InvalidOperationException("Request not found.");

            if (request.Status != FilprideMasterFileRequestStatus.ForApproval)
            {
                throw new InvalidOperationException("Only pending requests can be rejected.");
            }

            request.Status = FilprideMasterFileRequestStatus.Rejected;
            request.ApprovedBy = approver;
            request.ApprovedDate = DateTimeHelper.GetCurrentPhilippineTime();
            request.Remarks = remarks.Trim();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ApproveAsync(int id, string approver, string? remarks, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            bool invalidateChartCache = false;

            try
            {
                var request = await _dbContext.FilprideMasterFileRequests
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
                    ?? throw new InvalidOperationException("Request not found.");

                if (request.Status != FilprideMasterFileRequestStatus.ForApproval)
                {
                    throw new InvalidOperationException("Only pending requests can be approved.");
                }

                string activity = request.MasterFileType switch
                {
                    FilprideMasterFileType.Customer => await AddCustomerAsync(request, approver, cancellationToken),
                    FilprideMasterFileType.CustomerBranch => await AddCustomerBranchAsync(request, cancellationToken),
                    FilprideMasterFileType.Supplier => await AddSupplierAsync(request, approver, cancellationToken),
                    FilprideMasterFileType.BankAccount => await AddBankAccountAsync(request, approver, cancellationToken),
                    FilprideMasterFileType.Service => await AddServiceAsync(request, approver, cancellationToken),
                    FilprideMasterFileType.ChartOfAccount => await AddChartOfAccountAsync(request, approver, cancellationToken),
                    FilprideMasterFileType.PickupPoint => await AddPickupPointAsync(request, approver, cancellationToken),
                    _ => throw new InvalidOperationException("Unsupported master-file request type.")
                };

                invalidateChartCache = request.MasterFileType is FilprideMasterFileType.Supplier or FilprideMasterFileType.ChartOfAccount;
                request.Status = FilprideMasterFileRequestStatus.Approved;
                request.ApprovedBy = approver;
                request.ApprovedDate = DateTimeHelper.GetCurrentPhilippineTime();
                request.Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();

                _dbContext.FilprideAuditTrails.Add(new FilprideAuditTrail(approver, activity, request.MasterFileType.ToString())
                {
                    Id = Guid.NewGuid()
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                throw;
            }

            if (invalidateChartCache)
            {
                await _cacheService.RemoveByPrefixAsync("coa:", cancellationToken);
            }
        }

        private async Task<string> AddCustomerAsync(FilprideMasterFileRequest request, string approver, CancellationToken cancellationToken)
        {
            var payload = Deserialize<CustomerRequestPayload>(request);
            Require(payload.CustomerName, "Customer name");
            Require(payload.CustomerAddress, "Customer address");
            Require(payload.CustomerTin, "Customer TIN");
            Require(payload.CustomerTerms, "Customer terms");
            Require(payload.CustomerType, "Customer type");
            Require(payload.VatType, "VAT type");
            // TINs are intentionally not unique because missing requester TINs use a shared default value.

            var model = ToModel(payload);
            model.CustomerCode = await _unitOfWork.FilprideCustomer.GenerateCodeAsync(model.CustomerType, cancellationToken);
            model.CreatedBy = approver;
            _dbContext.FilprideCustomers.Add(model);
            return $"Created new Customer #{model.CustomerCode} from request #{request.Id}";
        }

        private async Task<string> AddCustomerBranchAsync(FilprideMasterFileRequest request, CancellationToken cancellationToken)
        {
            var payload = Deserialize<CustomerBranchRequestPayload>(request);
            Require(payload.BranchName, "Branch name");
            Require(payload.BranchAddress, "Branch address");
            Require(payload.BranchTin, "Branch TIN");
            var customer = await _dbContext.FilprideCustomers
                .FirstOrDefaultAsync(c => c.CustomerId == payload.CustomerId, cancellationToken)
                ?? throw new InvalidOperationException("The selected customer no longer exists.");

            customer.HasBranch = true;
            _dbContext.FilprideCustomerBranches.Add(ToModel(payload));
            return $"Created Customer Branch {payload.BranchName} from request #{request.Id}";
        }

        private async Task<string> AddSupplierAsync(FilprideMasterFileRequest request, string approver, CancellationToken cancellationToken)
        {
            var payload = Deserialize<SupplierRequestPayload>(request);
            Require(payload.SupplierName, "Supplier name");
            Require(payload.SupplierAddress, "Supplier address");
            Require(payload.SupplierTin, "Supplier TIN");
            Require(payload.SupplierTerms, "Supplier terms");
            Require(payload.Category, "Supplier category");
            if (payload.Category.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                Require(payload.EmployeeNumber, "Employee number");
            }
            // TINs are intentionally not unique because missing requester TINs use a shared default value.

            if (await _unitOfWork.FilprideSupplier.IsSupplierExistAsync(payload.SupplierName, payload.Category, cancellationToken))
            {
                throw new InvalidOperationException("A supplier with the same name and category already exists.");
            }

            var model = ToModel(payload);
            model.SupplierCode = await _unitOfWork.FilprideSupplier.GenerateCodeAsync(cancellationToken);
            model.CreatedBy = approver;
            _dbContext.FilprideSuppliers.Add(model);
            return $"Created new Supplier #{model.SupplierCode} from request #{request.Id}";
        }

        private async Task<string> AddBankAccountAsync(FilprideMasterFileRequest request, string approver, CancellationToken cancellationToken)
        {
            var payload = Deserialize<BankAccountRequestPayload>(request);
            Require(payload.Bank, "Bank");
            Require(payload.Branch, "Branch");
            Require(payload.AccountNo, "Account number");
            Require(payload.AccountName, "Account name");
            if (await _unitOfWork.FilprideBankAccount.IsBankAccountNoExist(payload.AccountNo, cancellationToken))
            {
                throw new InvalidOperationException("Bank account number already exists.");
            }
            if (await _unitOfWork.FilprideBankAccount.IsBankAccountNameExist(payload.AccountName, cancellationToken))
            {
                throw new InvalidOperationException("Bank account name already exists.");
            }

            var model = ToModel(payload);
            model.CreatedBy = approver;
            _dbContext.FilprideBankAccounts.Add(model);
            return $"Created new bank {model.Bank} {model.AccountName} {model.AccountNo} from request #{request.Id}";
        }

        private async Task<string> AddServiceAsync(FilprideMasterFileRequest request, string approver, CancellationToken cancellationToken)
        {
            var payload = Deserialize<ServiceRequestPayload>(request);
            Require(payload.Name, "Service name");
            if (await _unitOfWork.FilprideService.IsServicesExist(payload.Name, cancellationToken))
            {
                throw new InvalidOperationException("Service already exists.");
            }

            var current = await GetEligibleServiceAccountAsync(
                payload.CurrentAndPreviousId, "current and previous", cancellationToken);
            var unearned = await GetEligibleServiceAccountAsync(
                payload.UnearnedId, "unearned", cancellationToken);

            var model = ToModel(payload);
            model.ServiceNo = await _unitOfWork.FilprideService.GetLastNumber(cancellationToken);
            model.CurrentAndPreviousNo = current.AccountNumber;
            model.CurrentAndPreviousTitle = current.AccountName;
            model.UnearnedNo = unearned.AccountNumber;
            model.UnearnedTitle = unearned.AccountName;
            model.CreatedBy = approver;
            _dbContext.FilprideServices.Add(model);
            return $"Created Service #{model.ServiceNo} from request #{request.Id}";
        }

        private async Task<string> AddChartOfAccountAsync(FilprideMasterFileRequest request, string approver, CancellationToken cancellationToken)
        {
            var payload = Deserialize<ChartOfAccountRequestPayload>(request);
            string accountName = Require(payload.AccountName, "Account name").Trim();
            var parent = await _dbContext.FilprideChartOfAccounts.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.AccountId == payload.ParentAccountId, cancellationToken)
                ?? throw new InvalidOperationException("Parent account not found.");
            int level = parent.Level + 1;
            if (level is < 4 or > 5)
            {
                throw new InvalidOperationException("Only Level 4 and Level 5 accounts can be created.");
            }

            string accountNumber = await GenerateNextAccountNumberAsync(parent, cancellationToken);
            parent.HasChildren = true;
            _dbContext.FilprideChartOfAccounts.Add(new FilprideChartOfAccount
            {
                IsMain = false,
                AccountType = parent.AccountType,
                NormalBalance = parent.NormalBalance,
                AccountName = accountName,
                ParentAccountId = parent.AccountId,
                CreatedBy = approver,
                Level = level,
                FinancialStatementType = parent.FinancialStatementType,
                AccountNumber = accountNumber
            });
            return $"Created new Account #{accountNumber} from request #{request.Id}";
        }

        private async Task<string> AddPickupPointAsync(FilprideMasterFileRequest request, string approver, CancellationToken cancellationToken)
        {
            var payload = Deserialize<PickupPointRequestPayload>(request);
            Require(payload.Depot, "Depot");
            bool supplierExists = await _dbContext.FilprideSuppliers
                .AnyAsync(s => s.SupplierId == payload.SupplierId, cancellationToken);
            if (!supplierExists)
            {
                throw new InvalidOperationException("The selected supplier no longer exists.");
            }

            var model = ToModel(payload);
            model.CreatedBy = approver;
            model.CreatedDate = DateTimeHelper.GetCurrentPhilippineTime();
            _dbContext.FilpridePickUpPoints.Add(model);
            return $"Created Pickup Point {model.Depot} from request #{request.Id}";
        }

        private string SerializePayload(FilprideMasterFileType type, object model)
        {
            object payload = (type, model) switch
            {
                (FilprideMasterFileType.Customer, FilprideCustomer value) => new CustomerRequestPayload(
                    value.CustomerName, value.CustomerAddress, value.CustomerTin, value.BusinessStyle,
                    value.CustomerTerms, value.CustomerType, value.VatType, value.WithHoldingVat,
                    value.WithHoldingTax, value.ClusterCode, value.StationCode, value.CreditLimit,
                    value.CreditLimitAsOfToday, value.ZipCode, value.RetentionRate, value.HasMultipleTerms,
                    value.Type, value.RequiresPriceAdjustment, value.CommissioneeId, value.CommissionRate,
                    value.CwtPercent, value.CwVatPercent),
                (FilprideMasterFileType.CustomerBranch, FilprideCustomerBranch value) =>
                    new CustomerBranchRequestPayload(value.CustomerId, value.BranchName, value.BranchAddress, value.BranchTin),
                (FilprideMasterFileType.Supplier, FilprideSupplier value) => new SupplierRequestPayload(
                    value.SupplierName, value.SupplierAddress, value.SupplierTin, value.SupplierTerms,
                    value.VatType, value.TaxType, value.Category, value.EmployeeNumber, value.TradeName,
                    value.Branch, value.DefaultExpenseNumber, value.WithholdingTaxPercent,
                    value.WithholdingTaxTitle, value.ReasonOfExemption, value.Validity, value.ValidityDate,
                    value.ZipCode, value.RequiresPriceAdjustment, value.ProofOfRegistrationFilePath,
                    value.ProofOfRegistrationFileName, value.ProofOfExemptionFilePath, value.ProofOfExemptionFileName),
                (FilprideMasterFileType.BankAccount, FilprideBankAccount value) =>
                    new BankAccountRequestPayload(value.Bank, value.Branch, value.AccountNo, value.AccountName),
                (FilprideMasterFileType.Service, FilprideService value) =>
                    new ServiceRequestPayload(value.Name, value.CurrentAndPreviousId, value.UnearnedId, value.Percent),
                (FilprideMasterFileType.ChartOfAccount, ChartOfAccountRequestPayload value) => value,
                (FilprideMasterFileType.PickupPoint, FilpridePickUpPoint value) =>
                    new PickupPointRequestPayload(value.Depot, value.SupplierId),
                _ => throw new InvalidOperationException("The submitted model does not match the request type.")
            };

            return JsonSerializer.Serialize(payload, payload.GetType(), JsonOptions);
        }

        private static void ValidateSubmission(FilprideMasterFileType type, object model)
        {
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true))
            {
                throw new InvalidOperationException(string.Join(" ", validationResults.Select(result => result.ErrorMessage)));
            }

            switch (type, model)
            {
                case (FilprideMasterFileType.Customer, FilprideCustomer customer):
                    Require(customer.CustomerName, "Customer name");
                    Require(customer.CustomerAddress, "Customer address");
                    Require(customer.CustomerTin, "Customer TIN");
                    Require(customer.CustomerTerms, "Customer terms");
                    Require(customer.CustomerType, "Customer type");
                    Require(customer.VatType, "VAT type");
                    break;
                case (FilprideMasterFileType.CustomerBranch, FilprideCustomerBranch branch):
                    Require(branch.BranchName, "Branch name");
                    Require(branch.BranchAddress, "Branch address");
                    Require(branch.BranchTin, "Branch TIN");
                    if (branch.CustomerId == 0)
                    {
                        throw new InvalidOperationException("Customer is required.");
                    }
                    break;
                case (FilprideMasterFileType.Supplier, FilprideSupplier supplier):
                    Require(supplier.SupplierName, "Supplier name");
                    Require(supplier.SupplierAddress, "Supplier address");
                    Require(supplier.SupplierTin, "Supplier TIN");
                    Require(supplier.SupplierTerms, "Supplier terms");
                    Require(supplier.VatType, "VAT type");
                    Require(supplier.TaxType, "Tax type");
                    Require(supplier.Category, "Supplier category");
                    if (string.Equals(supplier.Category, "Employee", StringComparison.OrdinalIgnoreCase))
                    {
                        Require(supplier.EmployeeNumber, "Employee number");
                    }
                    break;
                case (FilprideMasterFileType.BankAccount, FilprideBankAccount bank):
                    Require(bank.Bank, "Bank");
                    Require(bank.Branch, "Branch");
                    Require(bank.AccountNo, "Account number");
                    Require(bank.AccountName, "Account name");
                    break;
                case (FilprideMasterFileType.Service, FilprideService service):
                    Require(service.Name, "Service name");
                    if (service.CurrentAndPreviousId == 0 || service.UnearnedId == 0)
                    {
                        throw new InvalidOperationException("Both service accounts are required.");
                    }
                    break;
                case (FilprideMasterFileType.ChartOfAccount, ChartOfAccountRequestPayload account):
                    Require(account.AccountName, "Account name");
                    if (account.ParentAccountId == 0)
                    {
                        throw new InvalidOperationException("Parent account is required.");
                    }
                    break;
                case (FilprideMasterFileType.PickupPoint, FilpridePickUpPoint pickup):
                    Require(pickup.Depot, "Depot");
                    if (pickup.SupplierId == 0)
                    {
                        throw new InvalidOperationException("Supplier is required.");
                    }
                    break;
                default:
                    throw new InvalidOperationException("The submitted model does not match the request type.");
            }
        }

        private async Task ValidateReferencesAsync(
            FilprideMasterFileType type,
            object model,
            CancellationToken cancellationToken)
        {
            if (type == FilprideMasterFileType.Service && model is FilprideService service)
            {
                await GetEligibleServiceAccountAsync(
                    service.CurrentAndPreviousId, "current and previous", cancellationToken);
                await GetEligibleServiceAccountAsync(service.UnearnedId, "unearned", cancellationToken);
            }
        }

        private async Task<FilprideChartOfAccount> GetEligibleServiceAccountAsync(
            int id,
            string accountName,
            CancellationToken cancellationToken) =>
            await _dbContext.FilprideChartOfAccounts
                .FirstOrDefaultAsync(c => c.AccountId == id && (c.Level == 4 || c.Level == 5), cancellationToken)
            ?? throw new InvalidOperationException($"The {accountName} account is not eligible for services.");

        private static T Deserialize<T>(FilprideMasterFileRequest request) =>
            JsonSerializer.Deserialize<T>(request.PayloadJson, JsonOptions)
            ?? throw new InvalidOperationException("The request payload is invalid.");

        private async Task<string> GenerateNextAccountNumberAsync(FilprideChartOfAccount parent, CancellationToken cancellationToken)
        {
            string? last = await _dbContext.FilprideChartOfAccounts.IgnoreQueryFilters()
                .Where(c => c.ParentAccountId == parent.AccountId && c.AccountNumber != null)
                .OrderByDescending(c => c.AccountNumber!.Length)
                .ThenByDescending(c => c.AccountNumber)
                .Select(c => c.AccountNumber)
                .FirstOrDefaultAsync(cancellationToken);
            string source = last ?? parent.AccountNumber
                ?? throw new InvalidOperationException("Parent account number is required.");
            if (!long.TryParse(source, out long series))
            {
                throw new InvalidOperationException($"Invalid account number format: {source}");
            }
            return (series + (parent.Level + 1 == 4 ? 100 : 1)).ToString();
        }

        private static string Require(string? value, string fieldName) =>
            string.IsNullOrWhiteSpace(value)
                ? throw new InvalidOperationException($"{fieldName} is required.")
                : value;

        private static FilprideCustomer ToModel(CustomerRequestPayload p) => new()
        {
            CustomerName = p.CustomerName, CustomerAddress = p.CustomerAddress, CustomerTin = p.CustomerTin,
            BusinessStyle = p.BusinessStyle, CustomerTerms = p.CustomerTerms, CustomerType = p.CustomerType,
            VatType = p.VatType, WithHoldingVat = p.WithHoldingVat, WithHoldingTax = p.WithHoldingTax,
            ClusterCode = p.ClusterCode, StationCode = p.StationCode, CreditLimit = p.CreditLimit,
            CreditLimitAsOfToday = p.CreditLimitAsOfToday, ZipCode = p.ZipCode, RetentionRate = p.RetentionRate,
            HasMultipleTerms = p.HasMultipleTerms, Type = p.Type, RequiresPriceAdjustment = p.RequiresPriceAdjustment,
            CommissioneeId = p.CommissioneeId, CommissionRate = p.CommissionRate,
            CwtPercent = p.CwtPercent, CwVatPercent = p.CwVatPercent
        };

        private static FilprideCustomerBranch ToModel(CustomerBranchRequestPayload p) => new()
        {
            CustomerId = p.CustomerId, BranchName = p.BranchName, BranchAddress = p.BranchAddress, BranchTin = p.BranchTin
        };

        private static FilprideSupplier ToModel(SupplierRequestPayload p) => new()
        {
            SupplierName = p.SupplierName, SupplierAddress = p.SupplierAddress, SupplierTin = p.SupplierTin,
            SupplierTerms = p.SupplierTerms, VatType = p.VatType, TaxType = p.TaxType, Category = p.Category,
            EmployeeNumber = p.EmployeeNumber, TradeName = p.TradeName, Branch = p.Branch,
            DefaultExpenseNumber = p.DefaultExpenseNumber, WithholdingTaxPercent = p.WithholdingTaxPercent,
            WithholdingTaxTitle = p.WithholdingTaxTitle, ReasonOfExemption = p.ReasonOfExemption,
            Validity = p.Validity, ValidityDate = p.ValidityDate, ZipCode = p.ZipCode,
            RequiresPriceAdjustment = p.RequiresPriceAdjustment,
            ProofOfRegistrationFilePath = p.ProofOfRegistrationFilePath,
            ProofOfRegistrationFileName = p.ProofOfRegistrationFileName,
            ProofOfExemptionFilePath = p.ProofOfExemptionFilePath,
            ProofOfExemptionFileName = p.ProofOfExemptionFileName
        };

        private static FilprideBankAccount ToModel(BankAccountRequestPayload p) => new()
        {
            Bank = p.Bank, Branch = p.Branch, AccountNo = p.AccountNo, AccountName = p.AccountName
        };

        private static FilprideService ToModel(ServiceRequestPayload p) => new()
        {
            Name = p.Name, CurrentAndPreviousId = p.CurrentAndPreviousId, UnearnedId = p.UnearnedId, Percent = p.Percent
        };

        private static FilpridePickUpPoint ToModel(PickupPointRequestPayload p) => new()
        {
            Depot = p.Depot, SupplierId = p.SupplierId
        };
    }
}
