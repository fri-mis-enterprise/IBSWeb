using System.Linq.Dynamic.Core;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models;
using IBS.Models.Enums;
using IBS.Models.MasterFile;
using IBS.Services.Attributes;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IBSWeb.Areas.Filpride.Controllers
{
    [Area(nameof(Filpride))]
    [CompanyAuthorize(nameof(Filpride))]
    public class DepartmentAccessConfigurationController: Controller
    {
        private readonly ApplicationDbContext _dbContext;


        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DisbursementController> _logger;

        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentAccessConfigurationController(ApplicationDbContext dbContext,
            IUnitOfWork unitOfWork,
            ILogger<DisbursementController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
        }

        private string GetUserFullName()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                   ?? User.Identity?.Name!;
        }

        private async Task<string?> GetCompanyClaimAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return null;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.FirstOrDefault(c => c.Type == "Company")?.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetDepartments([FromForm] DataTablesParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var department = _unitOfWork.DepartmentAccess
                    .GetAllQuery();

                var totalRecords = await department.CountAsync(cancellationToken);

                // Search filter
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    department = department
                        .Where(s =>
                            s.Department.Any(d => d.ToLower().Contains(searchValue)) ||
                            s.Module.ToLower().Contains(searchValue) ||
                            s.Action.ToLower().Contains(searchValue) ||
                            s.CreatedBy.ToLower().Contains(searchValue) ||
                            s.EditedBy.ToLower().Contains(searchValue)
                            );
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Name;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    department = department
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await department.CountAsync(cancellationToken);

                var pagedData = await department
                    .Skip(parameters.Start)
                    .Take(parameters.Length)
                    .Select(x => new
                    {
                        x.Id,
                        x.Department,
                        x.Module,
                        x.Action,
                        x.CreatedBy,
                        x.CreatedDate,
                        x.EditedBy,
                        x.EditedDate
                    })
                    .ToListAsync(cancellationToken);

                return Json(new
                {
                    draw = parameters.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalFilteredRecords,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get department access. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentAccessViewModel viewModel, CancellationToken cancellationToken)
        {

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region Saving Default Entries

                var validateIfModuleActionIsAlreadyExist = await _unitOfWork.DepartmentAccess.GetAllAsync(x => x.Action == viewModel.Action, cancellationToken);
                if (validateIfModuleActionIsAlreadyExist.Any())
                {
                    TempData["error"] = "The action for this module has already been created. Please edit the existing entry to add access.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new DepartmentAccess
                {
                    Department = viewModel.Department,
                    Module = viewModel.Module,
                    Action = viewModel.Action,
                    CreatedBy = GetUserFullName(),
                    CreatedDate = DateTimeHelper.GetCurrentPhilippineTime()
                };

                await _dbContext.DepartmentAccesses.AddAsync(model, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Module access created successfully";
                return RedirectToAction(nameof(Index));

                #endregion Saving Default Entries
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create access. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Invalid ID.");
                }

                var existingModel = await _unitOfWork.DepartmentAccess.GetAsync(x => x.Id == id, cancellationToken);

                if (existingModel == null)
                {
                    return NotFound();
                }

                var viewModel = new DepartmentAccessViewModel
                {
                    Id = existingModel.Id,
                    Department = existingModel.Department,
                    Module = existingModel.Module,
                    Action = existingModel.Action
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to fetch department access. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentAccessViewModel viewModel, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingRecord = await _unitOfWork.DepartmentAccess.GetAsync(x => x.Id == viewModel.Id, cancellationToken);

                if (existingRecord == null)
                {
                    return NotFound();
                }

                existingRecord.Module = viewModel.Module;
                existingRecord.Action = viewModel.Action;
                existingRecord.Department = viewModel.Department;
                existingRecord.EditedBy = GetUserFullName();
                existingRecord.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                await _unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Department access updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit department access. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        public IActionResult GetModuleAccess(string moduleName)
        {
            List<string> moduleAccessList = moduleName switch
            {
                nameof(CustomerOrderSlip) => Enum.GetNames<CustomerOrderSlip>().ToList(),
                nameof(DeliveryReceipts) => Enum.GetNames<DeliveryReceipts>().ToList(),
                nameof(SalesInvoice) => Enum.GetNames<SalesInvoice>().ToList(),
                nameof(ServiceInvoice) => Enum.GetNames<ServiceInvoice>().ToList(),
                nameof(CollectionReceipt) => Enum.GetNames<CollectionReceipt>().ToList(),
                nameof(DebitMemo) => Enum.GetNames<DebitMemo>().ToList(),
                nameof(CreditMemo) => Enum.GetNames<CreditMemo>().ToList(),
                nameof(AuthorityToLoad) => Enum.GetNames<AuthorityToLoad>().ToList(),
                nameof(PurchaseOrder) => Enum.GetNames<PurchaseOrder>().ToList(),
                nameof(ReceivingReport) => Enum.GetNames<ReceivingReport>().ToList(),
                nameof(CheckVoucherTrade) => Enum.GetNames<CheckVoucherTrade>().ToList(),
                nameof(CheckVoucherNonTradeInvoice) => Enum.GetNames<CheckVoucherNonTradeInvoice>().ToList(),
                nameof(CheckVoucherNonTradePayrollInvoice) => Enum.GetNames<CheckVoucherNonTradePayrollInvoice>().ToList(),
                nameof(CheckVoucherNonTradePayment) => Enum.GetNames<CheckVoucherNonTradePayment>().ToList(),
                nameof(JournalVoucher) => Enum.GetNames<JournalVoucher>().ToList(),
                nameof(Disbursement) => Enum.GetNames<Disbursement>().ToList(),
                nameof(ProvisionalReceipt) => Enum.GetNames<ProvisionalReceipt>().ToList(),
                _ => new List<string>()
            };

            return Json(moduleAccessList);
        }
    }
}
