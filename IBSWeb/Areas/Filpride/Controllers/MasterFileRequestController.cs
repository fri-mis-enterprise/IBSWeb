using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Enums;
using IBS.Models.Filpride.MasterFile;
using IBS.Services;
using IBS.Services.Attributes;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IBSWeb.Areas.Filpride.Controllers
{
    [Area(nameof(Filpride))]
    [Authorize]
    public class MasterFileRequestController : Controller
    {
        private const string ApproverRoles = "Admin,ManagementAccountingManager";
        private const int PageSize = 100;
        private static readonly string[] AllowedUploadExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
        private const long MaximumUploadSize = 10 * 1024 * 1024;
        private readonly MasterFileRequestService _requestService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudStorageService _cloudStorageService;

        public MasterFileRequestController(
            MasterFileRequestService requestService,
            ApplicationDbContext dbContext,
            IUnitOfWork unitOfWork,
            ICloudStorageService cloudStorageService)
        {
            _requestService = requestService;
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _cloudStorageService = cloudStorageService;
        }

        public async Task<IActionResult> Index(
            FilprideMasterFileRequestStatus? status,
            FilprideMasterFileType? type,
            string? search,
            CancellationToken cancellationToken,
            int page = 1)
        {
            string userId = GetUserId();
            IQueryable<FilprideMasterFileRequest> query = _requestService.GetRequests();
            if (!IsApprover())
            {
                query = query.Where(r => r.RequestedBy == userId);
            }
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status);
            }
            if (type.HasValue)
            {
                query = query.Where(r => r.MasterFileType == type);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchTerm = search.Trim();
                query = query.Where(r =>
                    EF.Functions.ILike(r.RequestedByName, $"%{searchTerm}%")
                    || EF.Functions.ILike(r.PayloadJson, $"%{searchTerm}%"));
            }
            int totalRequests = await query.CountAsync(cancellationToken);
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalRequests / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);
            ViewBag.Status = status;
            ViewBag.Type = type;
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            var requests = await query.OrderByDescending(r => r.RequestedDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync(cancellationToken);
            return View(requests);
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var request = await _requestService.GetAsync(id, cancellationToken);
            if (request == null)
            {
                return NotFound();
            }
            if (!IsApprover() && request.RequestedBy != GetUserId())
            {
                return Forbid();
            }
            ViewBag.ReferenceNames = await GetReferenceNamesAsync(request, cancellationToken);
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetCustomerDetails(int customerId, CancellationToken cancellationToken)
        {
            var customer = await _dbContext.FilprideCustomers
                .Where(c => c.CustomerId == customerId)
                .Select(c => new { address = c.CustomerAddress, tin = c.CustomerTin })
                .FirstOrDefaultAsync(cancellationToken);
            return customer == null ? NotFound() : Json(customer);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadSupplierDocument(
            int id,
            bool registration,
            CancellationToken cancellationToken)
        {
            var request = await _requestService.GetAsync(id, cancellationToken);
            if (request == null || request.MasterFileType != FilprideMasterFileType.Supplier)
            {
                return NotFound();
            }
            if (!IsApprover() && request.RequestedBy != GetUserId())
            {
                return Forbid();
            }

            var supplier = (FilprideSupplier)_requestService.DeserializeModel(request);
            string? fileName = registration
                ? supplier.ProofOfRegistrationFileName
                : supplier.ProofOfExemptionFileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }

            return Redirect(await _cloudStorageService.GetSignedUrlAsync(fileName));
        }

        [HttpGet]
        public async Task<IActionResult> CreateCustomer(CancellationToken cancellationToken)
        {
            var model = new FilprideCustomer();
            await PopulateCustomerListsAsync(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(FilprideCustomer model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCustomerListsAsync(model, cancellationToken);
                return View(model);
            }
            return await SaveAsync(FilprideMasterFileType.Customer, model, null, cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCustomerBranch(CancellationToken cancellationToken)
        {
            var model = new FilprideCustomerBranch();
            await PopulateCustomerBranchListAsync(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomerBranch(FilprideCustomerBranch model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCustomerBranchListAsync(model, cancellationToken);
                return View(model);
            }
            return await SaveAsync(FilprideMasterFileType.CustomerBranch, model, null, cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSupplier(CancellationToken cancellationToken)
        {
            var model = new FilprideSupplier();
            await PopulateSupplierListsAsync(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSupplier(
            FilprideSupplier model,
            IFormFile? registration,
            IFormFile? document,
            CancellationToken cancellationToken)
        {
            ValidateSupplier(model);
            ValidateUpload(registration, nameof(registration));
            ValidateUpload(document, nameof(document));
            if (!ModelState.IsValid)
            {
                await PopulateSupplierListsAsync(model, cancellationToken);
                return View(model);
            }

            await UploadSupplierDocumentsAsync(model, registration, document);
            return await SaveAsync(FilprideMasterFileType.Supplier, model, null, cancellationToken,
                model.ProofOfRegistrationFileName, model.ProofOfExemptionFileName);
        }

        [HttpGet]
        public IActionResult CreateBankAccount() => View(new FilprideBankAccount());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBankAccount(FilprideBankAccount model, CancellationToken cancellationToken) =>
            ModelState.IsValid
                ? await SaveAsync(FilprideMasterFileType.BankAccount, model, null, cancellationToken)
                : View(model);

        [HttpGet]
        public async Task<IActionResult> CreateService(CancellationToken cancellationToken)
        {
            var model = new FilprideService();
            await PopulateServiceListsAsync(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(FilprideService model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await PopulateServiceListsAsync(model, cancellationToken);
                return View(model);
            }
            return await SaveAsync(FilprideMasterFileType.Service, model, null, cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> CreateChartOfAccount(int? parentId, CancellationToken cancellationToken)
        {
            ViewBag.Parents = await GetAllowedParentAccountsAsync(cancellationToken);
            return View(new ChartOfAccountRequestPayload(parentId ?? 0, string.Empty));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChartOfAccount(ChartOfAccountRequestPayload model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid || model.ParentAccountId == 0 || string.IsNullOrWhiteSpace(model.AccountName))
            {
                ModelState.AddModelError(string.Empty, "Parent account and account name are required.");
                ViewBag.Parents = await GetAllowedParentAccountsAsync(cancellationToken);
                return View(model);
            }
            return await SaveAsync(FilprideMasterFileType.ChartOfAccount, model, null, cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePickupPoint(CancellationToken cancellationToken)
        {
            var model = new FilpridePickUpPoint();
            await PopulateSupplierListAsync(model, cancellationToken);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePickupPoint(FilpridePickUpPoint model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSupplierListAsync(model, cancellationToken);
                return View(model);
            }
            return await SaveAsync(FilprideMasterFileType.PickupPoint, model, null, cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var request = await _requestService.GetAsync(id, cancellationToken);
            if (request == null)
            {
                return NotFound();
            }
            if (request.RequestedBy != GetUserId())
            {
                return Forbid();
            }
            if (request.Status is not (FilprideMasterFileRequestStatus.ForApproval or FilprideMasterFileRequestStatus.Rejected))
            {
                TempData["error"] = "This request can no longer be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            object model = _requestService.DeserializeModel(request);
            await PopulateListsAsync(request.MasterFileType, model, cancellationToken);
            ViewBag.RequestId = id;
            return View($"Create{request.MasterFileType}", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditCustomer(int requestId, FilprideCustomer model, CancellationToken cancellationToken) =>
            EditAsync(FilprideMasterFileType.Customer, requestId, model, cancellationToken);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditCustomerBranch(int requestId, FilprideCustomerBranch model, CancellationToken cancellationToken) =>
            EditAsync(FilprideMasterFileType.CustomerBranch, requestId, model, cancellationToken);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSupplier(
            int requestId,
            FilprideSupplier model,
            IFormFile? registration,
            IFormFile? document,
            CancellationToken cancellationToken)
        {
            var request = await _requestService.GetAsync(requestId, cancellationToken);
            if (request == null)
            {
                return NotFound();
            }
            if (request.RequestedBy != GetUserId())
            {
                return Forbid();
            }
            if (request.MasterFileType != FilprideMasterFileType.Supplier
                || request.Status is not (FilprideMasterFileRequestStatus.ForApproval or FilprideMasterFileRequestStatus.Rejected))
            {
                TempData["error"] = "This request can no longer be edited.";
                return RedirectToAction(nameof(Details), new { id = requestId });
            }
            var previous = (FilprideSupplier)_requestService.DeserializeModel(request);
            model.ProofOfRegistrationFileName = previous.ProofOfRegistrationFileName;
            model.ProofOfRegistrationFilePath = previous.ProofOfRegistrationFilePath;
            model.ProofOfExemptionFileName = previous.ProofOfExemptionFileName;
            model.ProofOfExemptionFilePath = previous.ProofOfExemptionFilePath;
            ValidateSupplier(model);
            ValidateUpload(registration, nameof(registration));
            ValidateUpload(document, nameof(document));
            if (!ModelState.IsValid)
            {
                await PopulateSupplierListsAsync(model, cancellationToken);
                ViewBag.RequestId = requestId;
                return View("CreateSupplier", model);
            }
            await UploadSupplierDocumentsAsync(model, registration, document);
            return await SaveAsync(FilprideMasterFileType.Supplier, model, requestId, cancellationToken,
                registration == null ? null : model.ProofOfRegistrationFileName,
                document == null ? null : model.ProofOfExemptionFileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditBankAccount(int requestId, FilprideBankAccount model, CancellationToken cancellationToken) =>
            EditAsync(FilprideMasterFileType.BankAccount, requestId, model, cancellationToken);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditService(int requestId, FilprideService model, CancellationToken cancellationToken) =>
            EditAsync(FilprideMasterFileType.Service, requestId, model, cancellationToken);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditChartOfAccount(int requestId, ChartOfAccountRequestPayload model, CancellationToken cancellationToken) =>
            EditAsync(FilprideMasterFileType.ChartOfAccount, requestId, model, cancellationToken);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditPickupPoint(int requestId, FilpridePickUpPoint model, CancellationToken cancellationToken) =>
            EditAsync(FilprideMasterFileType.PickupPoint, requestId, model, cancellationToken);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _requestService.CancelAsync(id, GetUserId(), cancellationToken);
                TempData["success"] = "Request canceled.";
            }
            catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id, string? remarks, CancellationToken cancellationToken)
        {
            try
            {
                await _requestService.ApproveAsync(id, GetUserName(), remarks, cancellationToken);
                TempData["success"] = "Request approved and master file created.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["error"] = "This request was already processed by another approver.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject(int id, string remarks, CancellationToken cancellationToken)
        {
            try
            {
                await _requestService.RejectAsync(id, GetUserName(), remarks, cancellationToken);
                TempData["success"] = "Request rejected.";
            }
            catch (Exception ex) when (ex is InvalidOperationException or DbUpdateConcurrencyException)
            {
                TempData["error"] = ex is DbUpdateConcurrencyException
                    ? "This request was already processed by another approver."
                    : ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<IActionResult> SaveAsync(
            FilprideMasterFileType type,
            object model,
            int? requestId,
            CancellationToken cancellationToken,
            params string?[] uploadedFiles)
        {
            try
            {
                int id = await _requestService.SaveRequestAsync(
                    type, model, GetUserId(), GetUserName(), requestId, cancellationToken);
                TempData["success"] = requestId.HasValue ? "Request resubmitted for approval." : "Request submitted for approval.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (UnauthorizedAccessException)
            {
                await DeleteUploadedFilesAsync(uploadedFiles);
                return Forbid();
            }
            catch (DbUpdateConcurrencyException)
            {
                await DeleteUploadedFilesAsync(uploadedFiles);
                TempData["error"] = "This request changed while it was being processed. Please review it and try again.";
                return RedirectToAction(nameof(Details), new { id = requestId });
            }
            catch (InvalidOperationException ex)
            {
                await DeleteUploadedFilesAsync(uploadedFiles);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<IActionResult> EditAsync(
            FilprideMasterFileType type,
            int requestId,
            object model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await PopulateListsAsync(type, model, cancellationToken);
                ViewBag.RequestId = requestId;
                return View($"Create{type}", model);
            }

            return await SaveAsync(type, model, requestId, cancellationToken);
        }

        private async Task<IReadOnlyDictionary<string, string>> GetReferenceNamesAsync(
            FilprideMasterFileRequest request,
            CancellationToken cancellationToken)
        {
            switch (request.MasterFileType)
            {
                case FilprideMasterFileType.CustomerBranch:
                {
                    var payload = (CustomerBranchRequestPayload)_requestService.DeserializeModel(request);
                    string? customerName = await _dbContext.FilprideCustomers
                        .Where(c => c.CustomerId == payload.CustomerId)
                        .Select(c => c.CustomerName)
                        .FirstOrDefaultAsync(cancellationToken);
                    return customerName == null
                        ? new Dictionary<string, string>()
                        : new Dictionary<string, string> { ["CustomerId"] = customerName };
                }
                case FilprideMasterFileType.PickupPoint:
                {
                    var payload = (FilpridePickUpPoint)_requestService.DeserializeModel(request);
                    string? supplierName = await _dbContext.FilprideSuppliers
                        .Where(s => s.SupplierId == payload.SupplierId)
                        .Select(s => s.SupplierName)
                        .FirstOrDefaultAsync(cancellationToken);
                    return supplierName == null
                        ? new Dictionary<string, string>()
                        : new Dictionary<string, string> { ["SupplierId"] = supplierName };
                }
                case FilprideMasterFileType.ChartOfAccount:
                {
                    var payload = (ChartOfAccountRequestPayload)_requestService.DeserializeModel(request);
                    string? parentName = await _dbContext.FilprideChartOfAccounts
                        .Where(a => a.AccountId == payload.ParentAccountId)
                        .Select(a => a.AccountNumber + " " + a.AccountName)
                        .FirstOrDefaultAsync(cancellationToken);
                    return parentName == null
                        ? new Dictionary<string, string>()
                        : new Dictionary<string, string> { ["ParentAccountId"] = parentName };
                }
                case FilprideMasterFileType.Service:
                {
                    var payload = (FilprideService)_requestService.DeserializeModel(request);
                    var accounts = await _dbContext.FilprideChartOfAccounts
                        .Where(a => a.AccountId == payload.CurrentAndPreviousId || a.AccountId == payload.UnearnedId)
                        .Select(a => new { a.AccountId, Name = a.AccountNumber + " " + a.AccountName })
                        .ToDictionaryAsync(a => a.AccountId, a => a.Name, cancellationToken);
                    var referenceNames = new Dictionary<string, string>();
                    if (accounts.TryGetValue(payload.CurrentAndPreviousId, out string? currentAndPreviousName))
                    {
                        referenceNames["CurrentAndPreviousId"] = currentAndPreviousName;
                    }
                    if (accounts.TryGetValue(payload.UnearnedId, out string? unearnedName))
                    {
                        referenceNames["UnearnedId"] = unearnedName;
                    }
                    return referenceNames;
                }
                default:
                    return new Dictionary<string, string>();
            }
        }

        private async Task DeleteUploadedFilesAsync(IEnumerable<string?> uploadedFiles)
        {
            foreach (string? file in uploadedFiles.Where(file => !string.IsNullOrWhiteSpace(file)))
            {
                await _cloudStorageService.DeleteFileAsync(file!);
            }
        }

        private async Task PopulateListsAsync(FilprideMasterFileType type, object model, CancellationToken cancellationToken)
        {
            switch (type, model)
            {
                case (FilprideMasterFileType.Customer, FilprideCustomer customer):
                    await PopulateCustomerListsAsync(customer, cancellationToken);
                    break;
                case (FilprideMasterFileType.CustomerBranch, FilprideCustomerBranch branch):
                    await PopulateCustomerBranchListAsync(branch, cancellationToken);
                    break;
                case (FilprideMasterFileType.Supplier, FilprideSupplier supplier):
                    await PopulateSupplierListsAsync(supplier, cancellationToken);
                    break;
                case (FilprideMasterFileType.Service, FilprideService service):
                    await PopulateServiceListsAsync(service, cancellationToken);
                    break;
                case (FilprideMasterFileType.ChartOfAccount, ChartOfAccountRequestPayload):
                    ViewBag.Parents = await GetAllowedParentAccountsAsync(cancellationToken);
                    break;
                case (FilprideMasterFileType.PickupPoint, FilpridePickUpPoint point):
                    await PopulateSupplierListAsync(point, cancellationToken);
                    break;
            }
        }

        private async Task PopulateCustomerListsAsync(FilprideCustomer model, CancellationToken cancellationToken)
        {
            model.PaymentTerms = await _unitOfWork.FilprideTerms.GetFilprideTermsListAsyncByCode(cancellationToken);
            model.Commissionees = await _dbContext.FilprideSuppliers
                .Where(s => s.IsActive && s.Category == "Commissionee")
                .OrderBy(s => s.SupplierCode)
                .Select(s => new SelectListItem(s.SupplierCode + " " + s.SupplierName, s.SupplierId.ToString()))
                .ToListAsync(cancellationToken);
        }

        private async Task PopulateCustomerBranchListAsync(FilprideCustomerBranch model, CancellationToken cancellationToken) =>
            model.CustomerSelectList = await _dbContext.FilprideCustomers.Where(c => c.IsActive)
                .OrderBy(c => c.CustomerName)
                .Select(c => new SelectListItem(c.CustomerName, c.CustomerId.ToString()))
                .ToListAsync(cancellationToken);

        private async Task PopulateSupplierListsAsync(FilprideSupplier model, CancellationToken cancellationToken)
        {
            model.DefaultExpenses = await _dbContext.FilprideChartOfAccounts.Where(c => !c.HasChildren)
                .OrderBy(c => c.AccountNumber)
                .Select(c => new SelectListItem(c.AccountNumber + " " + c.AccountName, c.AccountNumber))
                .ToListAsync(cancellationToken);
            model.WithholdingTaxList = await _dbContext.FilprideChartOfAccounts
                .Where(c => c.AccountNumber!.Contains("2010302") && !c.HasChildren)
                .OrderBy(c => c.AccountNumber)
                .Select(c => new SelectListItem(c.AccountNumber + " " + c.AccountName, c.AccountNumber + " " + c.AccountName))
                .ToListAsync(cancellationToken);
            model.PaymentTerms = await _unitOfWork.FilprideTerms.GetFilprideTermsListAsyncByCode(cancellationToken);
        }

        private async Task PopulateServiceListsAsync(FilprideService model, CancellationToken cancellationToken)
        {
            var accounts = await _dbContext.FilprideChartOfAccounts.Where(c => c.Level == 4 || c.Level == 5)
                .OrderBy(c => c.AccountId)
                .Select(c => new SelectListItem(c.AccountNumber + " " + c.AccountName, c.AccountId.ToString()))
                .ToListAsync(cancellationToken);
            model.CurrentAndPreviousTitles = accounts;
            model.UnearnedTitles = accounts;
        }

        private async Task PopulateSupplierListAsync(FilpridePickUpPoint model, CancellationToken cancellationToken) =>
            model.Suppliers = await _dbContext.FilprideSuppliers.Where(s => s.IsActive && s.Category == "Trade")
                .OrderBy(s => s.SupplierCode)
                .Select(s => new SelectListItem(s.SupplierCode + " " + s.SupplierName, s.SupplierId.ToString()))
                .ToListAsync(cancellationToken);

        private async Task<List<SelectListItem>> GetAllowedParentAccountsAsync(CancellationToken cancellationToken) =>
            await _dbContext.FilprideChartOfAccounts.IgnoreQueryFilters()
                .Where(c => c.Level == 3 || c.Level == 4)
                .OrderBy(c => c.AccountNumber)
                .Select(c => new SelectListItem(c.AccountNumber + " " + c.AccountName, c.AccountId.ToString()))
                .ToListAsync(cancellationToken);

        private void ValidateSupplier(FilprideSupplier model)
        {
            if (string.Equals(model.Category, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(model.EmployeeNumber))
                {
                    ModelState.AddModelError(nameof(model.EmployeeNumber), "Employee Number is required.");
                }
                else
                {
                    model.EmployeeNumber = model.EmployeeNumber.Trim();
                }
            }
            else
            {
                model.EmployeeNumber = null;
            }
        }

        private void ValidateUpload(IFormFile? file, string fieldName)
        {
            if (file == null)
            {
                return;
            }
            if (file.Length == 0 || file.Length > MaximumUploadSize)
            {
                ModelState.AddModelError(fieldName, "Files must be non-empty and no larger than 10 MB.");
            }
            if (!AllowedUploadExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(fieldName, "Only PDF, JPG, and PNG files are allowed.");
            }
        }

        private async Task UploadSupplierDocumentsAsync(FilprideSupplier model, IFormFile? registration, IFormFile? document)
        {
            if (registration != null)
            {
                model.ProofOfRegistrationFileName = GenerateFileName(registration.FileName);
                model.ProofOfRegistrationFilePath = await _cloudStorageService.UploadFileAsync(registration, model.ProofOfRegistrationFileName);
            }
            if (document != null)
            {
                model.ProofOfExemptionFileName = GenerateFileName(document.FileName);
                model.ProofOfExemptionFilePath = await _cloudStorageService.UploadFileAsync(document, model.ProofOfExemptionFileName);
            }
        }

        private static string GenerateFileName(string incomingFileName)
        {
            string baseName = Path.GetFileNameWithoutExtension(incomingFileName);
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                baseName = baseName.Replace(invalidCharacter, '-');
            }
            baseName = baseName[..Math.Min(baseName.Length, 100)];
            return $"{baseName}-{DateTimeHelper.GetCurrentPhilippineTime():yyyyMMddHHmmss}-{Guid.NewGuid():N}{Path.GetExtension(incomingFileName).ToLowerInvariant()}";
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user identifier is missing.");

        private string GetUserName() => User.FindFirstValue(ClaimTypes.GivenName) ?? User.Identity!.Name!;

        private bool IsApprover() => User.IsInRole("Admin") || User.IsInRole("ManagementAccountingManager");
    }
}
