using System.Linq.Dynamic.Core;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Enums;
using IBS.Models.Filpride.AccountsPayable;
using IBS.Models.Filpride.Books;
using IBS.Models.Filpride.ViewModels;
using IBS.Models;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace IBSWeb.Areas.Filpride.Controllers
{
    [Area(nameof(Filpride))]
    [Authorize]
    public class ReceivingReportController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ReceivingReportController> _logger;

        private const string FilterTypeClaimType = "ReceivingReport.FilterType";

        public ReceivingReportController(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ILogger<ReceivingReportController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        private string GetUserFullName()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                   ?? User.Identity?.Name!;
        }

        private async Task UpdateFilterTypeClaim(string filterType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var existingClaim = (await _userManager.GetClaimsAsync(user))
                    .FirstOrDefault(c => c.Type == FilterTypeClaimType);

                if (existingClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, existingClaim);
                }

                if (!string.IsNullOrEmpty(filterType))
                {
                    await _userManager.AddClaimAsync(user, new Claim(FilterTypeClaimType, filterType));
                }
            }
        }

        private async Task<string?> GetCurrentFilterType()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.FirstOrDefault(c => c.Type == FilterTypeClaimType)?.Value;
        }

        public async Task<IActionResult> Index(string? view, string filterType)
        {
            await UpdateFilterTypeClaim(filterType);
            ViewBag.FilterType = filterType;
            if (view != nameof(DynamicView.ReceivingReport))
            {
                return View();
            }

            return View("ExportIndex");

            //For the function of correcting the journal entries
            // var receivingReportss = await _unitOfWork.FilprideReceivingReport
            //     .GetAllAsync();
            //
            // foreach (var receivingReports in receivingReportss)
            // {
            //     await Void(receivingReports.ReceivingReportId, cancellationToken);
            //     await Post(receivingReports.ReceivingReportId, cancellationToken);
            // }
        }

        [HttpPost]
        public async Task<IActionResult> GetReceivingReports([FromForm] DataTablesParameters parameters, DateOnly filterDate, CancellationToken cancellationToken)
        {
            try
            {

                var filterTypeClaim = await GetCurrentFilterType();

                var receivingReports = _unitOfWork.FilprideReceivingReport
                    .GetAllQuery(x => true);

                var totalRecords = await receivingReports.CountAsync(cancellationToken);

                if (!string.IsNullOrEmpty(filterTypeClaim))
                {
                    switch (filterTypeClaim)
                    {
                        case "RecordSupplierDetails":
                            receivingReports = receivingReports
                                .Where(rr => (rr.SupplierDrNo == null
                                              || rr.SupplierInvoiceDate == null
                                              || rr.SupplierInvoiceNumber == null
                                              || rr.SupplierDrNo == null
                                              || rr.WithdrawalCertificate == null
                                              || rr.CostBasedOnSoa == 0)
                                             && rr.CanceledBy == null
                                             && rr.VoidedBy == null);
                            break;
                            // Add other cases as needed
                    }
                }

                // Search filter
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();
                    var hasDate = DateOnly.TryParse(searchValue, out var date);

                    receivingReports = receivingReports
                    .Where(s =>
                        s.ReceivingReportNo!.ToLower().Contains(searchValue) ||
                        s.PurchaseOrder!.PurchaseOrderNo!.ToLower().Contains(searchValue) ||
                        s.DeliveryReceipt!.DeliveryReceiptNo.ToLower().Contains(searchValue) == true ||
                        (hasDate && s.Date == date) ||
                        s.QuantityReceived.ToString().Contains(searchValue) ||
                        s.Amount.ToString().Contains(searchValue) ||
                        s.CreatedBy!.ToLower().Contains(searchValue) ||
                        s.Remarks.ToLower().Contains(searchValue)
                        );
                }
                if (filterDate != DateOnly.MinValue && filterDate != default)
                {
                    receivingReports = receivingReports.Where(s => s.Date == filterDate);
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Name;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    receivingReports = receivingReports
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalFilteredRecords = await receivingReports.CountAsync(cancellationToken);

                var pagedData = await receivingReports
                    .Skip(parameters.Start)
                    .Take(parameters.Length)
                    .Select(rr => new
                    {
                        rr.ReceivingReportId,
                        rr.ReceivingReportNo,
                        rr.Date,
                        rr.PurchaseOrder!.PurchaseOrderNo,
                        rr.PurchaseOrder.OldPoNo,
                        rr.OldRRNo,
                        rr.DeliveryReceiptId,
                        rr.DeliveryReceipt!.DeliveryReceiptNo,
                        rr.DeliveryReceipt!.Customer!.CustomerName,
                        rr.PurchaseOrder!.Product!.ProductName,
                        rr.QuantityReceived,
                        rr.CreatedBy,
                        rr.Status,
                        rr.VoidedBy,
                        rr.PostedBy,
                        rr.CanceledBy,
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
                _logger.LogError(ex, "Failed to get receiving reports. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [Authorize(Policy = nameof(ReceivingReport.ReceivingReportCreate))]
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var viewModel = new ReceivingReportViewModel();

            ViewBag.FilterType = await GetCurrentFilterType();

            viewModel.PurchaseOrders = await _unitOfWork.FilpridePurchaseOrder
                .GetPurchaseOrderListAsyncById(cancellationToken);

            return View(viewModel);
        }

        [Authorize(Policy = nameof(ReceivingReport.ReceivingReportCreate))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceivingReportViewModel viewModel, CancellationToken cancellationToken)
        {

            viewModel.PurchaseOrders = await _unitOfWork.FilpridePurchaseOrder
                .GetPurchaseOrderListAsyncById(cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                #region --Retrieve PO

                var existingPo = await _unitOfWork.FilpridePurchaseOrder
                    .GetAsync(po => po.PurchaseOrderId == viewModel.PurchaseOrderId, cancellationToken);

                if (existingPo == null)
                {
                    return NotFound();
                }

                #endregion --Retrieve PO

                var totalAmountRr = existingPo.Quantity - existingPo.QuantityReceived;

                if (viewModel.QuantityDelivered > totalAmountRr)
                {
                    TempData["info"] = "Input is exceed to remaining quantity delivered";
                    return View(viewModel);
                }

                var model = new FilprideReceivingReport
                {
                    ReceivingReportNo = await _unitOfWork.FilprideReceivingReport.GenerateCodeAsync(existingPo.Type!, cancellationToken),
                    Date = viewModel.Date,
                    DueDate = await _unitOfWork.FilprideReceivingReport.ComputeDueDateAsync(existingPo.Terms, viewModel.Date, cancellationToken),
                    POId = existingPo.PurchaseOrderId,
                    PONo = existingPo.PurchaseOrderNo,
                    SupplierInvoiceNumber = viewModel.SupplierSiNo,
                    SupplierInvoiceDate = viewModel.SupplierSiDate,
                    TruckOrVessels = viewModel.TruckOrVessels,
                    QuantityReceived = viewModel.QuantityReceived,
                    QuantityDelivered = viewModel.QuantityDelivered,
                    GainOrLoss = viewModel.QuantityReceived - viewModel.QuantityDelivered,
                    Amount = DecimalRoundingHelper.ComputeAmountFromUnitPrice(
                        viewModel.QuantityReceived,
                        await _unitOfWork.FilpridePurchaseOrder.GetPurchaseOrderCost(existingPo.PurchaseOrderId, cancellationToken)),
                    AuthorityToLoadNo = viewModel.AuthorityToLoadNo,
                    Remarks = viewModel.Remarks,
                    CreatedBy = GetUserFullName(),
                    ReceivedDate = viewModel.ReceivedDate,
                    SupplierDrNo = viewModel.SupplierDrNo,
                    WithdrawalCertificate = viewModel.WithdrawalCertificate,
                    Type = existingPo.Type,
                    OldRRNo = viewModel.OldRRNo,
                };

                #region --Audit Trail Recording

                FilprideAuditTrail auditTrailBook = new(model.CreatedBy!, $"Create new receiving report# {model.ReceivingReportNo}", "Receiving Report");
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.FilprideReceivingReport.AddAsync(model, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = $"Receiving Report #{model.ReceivingReportNo} created successfully";
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to create receiving report. Error: {ErrorMessage}, Stack: {StackTrace}. Created by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [Authorize(Policy = nameof(ReceivingReport.ReceivingReportEdit))]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {

                ViewBag.FilterType = await GetCurrentFilterType();

                var receivingReport = await _unitOfWork.FilprideReceivingReport
                    .GetAsync(x => x.ReceivingReportId == id, cancellationToken);

                if (receivingReport == null)
                {
                    return NotFound();
                }

                var minDate =
                    await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.ReceivingReport,
                        cancellationToken);

                var viewModel = new ReceivingReportViewModel
                {
                    ReceivingReportId = receivingReport.ReceivingReportId,
                    Date = receivingReport.Date,
                    PurchaseOrderId = receivingReport.POId,
                    PurchaseOrders = await _unitOfWork.FilpridePurchaseOrder
                        .GetPurchaseOrderListAsyncById(cancellationToken),
                    ReceivedDate = receivingReport.ReceivedDate,
                    SupplierSiNo = receivingReport.SupplierInvoiceNumber,
                    SupplierSiDate = receivingReport.SupplierInvoiceDate,
                    SupplierDrNo = receivingReport.SupplierDrNo,
                    WithdrawalCertificate = receivingReport.WithdrawalCertificate,
                    TruckOrVessels = receivingReport.TruckOrVessels,
                    QuantityDelivered = receivingReport.QuantityDelivered,
                    QuantityReceived = receivingReport.QuantityReceived,
                    AuthorityToLoadNo = receivingReport.AuthorityToLoadNo,
                    Remarks = receivingReport.Remarks,
                    PostedBy = receivingReport.PostedBy,
                    CostBasedOnSoa = receivingReport.CostBasedOnSoa,
                    MinDate = minDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to fetch receiving report. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [Authorize(Policy = nameof(ReceivingReport.ReceivingReportEdit))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReceivingReportViewModel viewModel, CancellationToken cancellationToken)
        {
            var existingModel = await _unitOfWork.FilprideReceivingReport
                .GetAsync(x => x.ReceivingReportId == viewModel.ReceivingReportId, cancellationToken);

            if (existingModel == null)
            {
                return NotFound();
            }

            viewModel.PurchaseOrders = await _unitOfWork.FilpridePurchaseOrder
                .GetPurchaseOrderListAsyncById(cancellationToken);

            viewModel.MinDate = await _unitOfWork.GetMinimumPeriodBasedOnThePostedPeriods(Module.ReceivingReport, cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The information you submitted is not valid";
                return View(viewModel);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var isPosted = existingModel.PostedBy != null;

                existingModel.SupplierInvoiceNumber = viewModel.SupplierSiNo;
                existingModel.SupplierInvoiceDate = viewModel.SupplierSiDate;
                existingModel.SupplierDrNo = viewModel.SupplierDrNo;
                existingModel.WithdrawalCertificate = viewModel.WithdrawalCertificate;
                existingModel.TruckOrVessels = viewModel.TruckOrVessels;
                existingModel.Remarks = viewModel.Remarks;
                existingModel.CostBasedOnSoa = viewModel.CostBasedOnSoa;

                if (!isPosted)
                {
                    #region --Retrieve PO

                    var po = await _unitOfWork.FilpridePurchaseOrder
                        .GetAsync(x => x.PurchaseOrderId == viewModel.PurchaseOrderId, cancellationToken);

                    if (po == null)
                    {
                        return NotFound();
                    }

                    #endregion --Retrieve PO

                    var totalAmountRr = po.Quantity - po.QuantityReceived + existingModel.QuantityReceived;

                    if (viewModel.QuantityDelivered > totalAmountRr)
                    {
                        TempData["info"] = "Input is exceed to remaining quantity delivered";
                        return View(viewModel);
                    }

                    existingModel.Date = viewModel.Date;
                    existingModel.POId = po.PurchaseOrderId;
                    existingModel.PONo = po.PurchaseOrderNo;
                    existingModel.DueDate = await _unitOfWork.FilprideReceivingReport.ComputeDueDateAsync(po.Terms, viewModel.Date, cancellationToken);
                    existingModel.QuantityDelivered = viewModel.QuantityDelivered;
                    existingModel.QuantityReceived = viewModel.QuantityReceived;
                    existingModel.GainOrLoss = viewModel.QuantityReceived - viewModel.QuantityDelivered;
                    existingModel.AuthorityToLoadNo = viewModel.AuthorityToLoadNo;
                    existingModel.ReceivedDate = viewModel.ReceivedDate;
                    existingModel.Amount = DecimalRoundingHelper.ComputeAmountFromUnitPrice(
                        viewModel.QuantityReceived,
                        await _unitOfWork.FilpridePurchaseOrder.GetPurchaseOrderCost(po.PurchaseOrderId, cancellationToken));
                }

                if (!_dbContext.ChangeTracker.HasChanges())
                {
                    TempData["warning"] = "No data changes!";
                    return View(viewModel);
                }

                existingModel.EditedBy = GetUserFullName();
                existingModel.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                #region --Audit Trail Recording

                FilprideAuditTrail auditTrailBook = new(existingModel.EditedBy!, $"Edited receiving report# {existingModel.ReceivingReportNo}", "Receiving Report");
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Receiving Report updated successfully";
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit receiving report. Error: {ErrorMessage}, Stack: {StackTrace}. Edited by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                TempData["error"] = ex.Message;
                return View(viewModel);
            }
        }

        [Authorize(Policy = nameof(ReceivingReport.ReceivingReportPreview))]
        [HttpGet]
        public async Task<IActionResult> Print(int id, CancellationToken cancellationToken)
        {
            var receivingReport = await _unitOfWork.FilprideReceivingReport
                .GetAsync(rr => rr.ReceivingReportId == id, cancellationToken);

            ViewBag.FilterType = await GetCurrentFilterType();

            if (receivingReport == null)
            {
                return NotFound();
            }

            #region --Audit Trail Recording

            FilprideAuditTrail auditTrailBook = new(GetUserFullName(), $"Preview receiving report# {receivingReport.ReceivingReportNo}", "Purchase Order");
            await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

            #endregion --Audit Trail Recording

            return View(receivingReport);
        }

        [Authorize(Policy = nameof(ReceivingReport.ReceivingReportPost))]
        public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.FilprideReceivingReport
                .GetAsync(rr => rr.ReceivingReportId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            if (model.PostedBy != null)
            {
                TempData["info"] = "Receiving Report has already been posted.";
                return RedirectToAction(nameof(Print), new { id });
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (model.ReceivedDate == null)
                {
                    TempData["info"] = "Please indicate the received date.";
                    return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
                }

                model.PostedBy = GetUserFullName();
                model.PostedDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.Status = nameof(Status.Posted);

                #region --Audit Trail Recording

                FilprideAuditTrail auditTrailBook = new(model.PostedBy!, $"Posted receiving report# {model.ReceivingReportNo}", "Receiving Report");
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await _unitOfWork.FilprideReceivingReport.PostAsync(model, cancellationToken);
                await _unitOfWork.FilprideInventory.AddPurchaseToInventoryAsync(model, cancellationToken);

                await _unitOfWork.FilprideReceivingReport.UpdatePoAsync(model.PurchaseOrder!.PurchaseOrderId,
                    model.QuantityReceived, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Receiving Report has been posted.";
                return RedirectToAction(nameof(Print), new { id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to post receiving report. Error: {ErrorMessage}, Stack: {StackTrace}. Posted by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Void(int id, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.FilprideReceivingReport
                .GetAsync(rr => rr.ReceivingReportId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _unitOfWork.FilprideReceivingReport.VoidReceivingReportAsync(
                    model.ReceivingReportId,
                    GetUserFullName(), cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Receiving Report #{model.ReceivingReportNo} has been voided successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to void receiving report. Error: {ErrorMessage}, Stack: {StackTrace}. Voided by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                await transaction.RollbackAsync(cancellationToken);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Policy = nameof(ReceivingReport.ReceivingReportCancel))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? cancellationRemarks, CancellationToken cancellationToken)
        {
            var model = await _unitOfWork.FilprideReceivingReport
                .GetAsync(rr => rr.ReceivingReportId == id, cancellationToken);

            if (model == null)
            {
                return NotFound();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                model.CanceledBy = GetUserFullName();
                model.CanceledDate = DateTimeHelper.GetCurrentPhilippineTime();
                model.CanceledQuantity = model.QuantityDelivered < model.QuantityReceived ? model.QuantityDelivered : model.QuantityReceived;
                model.Status = nameof(Status.Canceled);
                model.CancellationRemarks = cancellationRemarks;

                #region --Audit Trail Recording

                FilprideAuditTrail auditTrailBook = new(model.CanceledBy!, $"Canceled receiving report# {model.ReceivingReportNo}", "Receiving Report");
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                await transaction.CommitAsync(cancellationToken);

                return Json(new { success = true, message = $"Receiving Report #{model.ReceivingReportNo} has been cancelled successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to cancel receiving report. Error: {ErrorMessage}, Stack: {StackTrace}. Canceled by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLiquidations(int id, CancellationToken cancellationToken)
        {
            var po = await _unitOfWork.FilpridePurchaseOrder
                .GetAsync(po => po.PurchaseOrderId == id, cancellationToken);

            if (po == null)
            {
                return NotFound();
            }

            var receivingReports = await _unitOfWork
                .FilprideReceivingReport
                .GetAllAsync(x => true
                                   && x.PONo == po.PurchaseOrderNo, cancellationToken);

            var rrList = receivingReports
                .Select(rr => new
                {
                    rr.ReceivingReportNo,
                    rr.QuantityReceived,
                    rr.QuantityDelivered,
                    string.Empty,
                    rr.PONo,
                    rr.Status
                })
                .ToList();

            var rrPostedOnly = rrList
                .Where(rr => true
                                   && rr.PONo == po.PurchaseOrderNo
                                   && rr.Status == nameof(Status.Posted))
                .ToList();

            var rrNotPosted = rrList
                .Where(rr => true
                                                    && rr.PONo == po.PurchaseOrderNo
                                                    && rr.Status == nameof(Status.Pending))
                .ToList();

            var rrCanceled = rrList
                .Where(rr => true
                             && rr.PONo == po.PurchaseOrderNo
                             && (rr.Status == nameof(Status.Canceled)
                                 || rr.Status == nameof(Status.Voided)))
                .ToList();

            return Json(new
            {
                poNo = po.PurchaseOrderNo,
                poQuantity = po.Quantity.ToString(SD.Two_Decimal_Format),
                rrList,
                rrListPostedOnly = rrPostedOnly,
                rrListNotPosted = rrNotPosted,
                rrListCanceled = rrCanceled
            });
        }

        [Authorize(Policy = nameof(ReceivingReport.ReceivingReportPreview))]
        public async Task<IActionResult> Printed(int id, CancellationToken cancellationToken)
        {
            var rr = await _unitOfWork.FilprideReceivingReport
                .GetAsync(x => x.ReceivingReportId == id, cancellationToken);

            if (rr == null)
            {
                return NotFound();
            }

            if (!rr.IsPrinted)
            {
                #region --Audit Trail Recording

                FilprideAuditTrail auditTrailBook = new(GetUserFullName(), $"Printed original copy of receiving report# {rr.ReceivingReportNo}", "Receiving Report");
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording

                rr.IsPrinted = true;
                await _unitOfWork.SaveAsync(cancellationToken);
            }
            else
            {
                #region --Audit Trail Recording

                FilprideAuditTrail auditTrailBook = new(GetUserFullName(), $"Printed re-printed copy of receiving report# {rr.ReceivingReportNo}", "Receiving Report");
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion --Audit Trail Recording
            }

            return RedirectToAction(nameof(Print), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetReceivingReportList(
            [FromForm] DataTablesParameters parameters,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken)
        {
            try
            {

                var receivingReports = await _unitOfWork.FilprideReceivingReport
                    .GetAllAsync(rr => rr.Type == nameof(DocumentType.Documented), cancellationToken);

                // Apply date range filter if provided
                if (dateFrom.HasValue)
                {
                    receivingReports = receivingReports
                        .Where(s => s.Date >= DateOnly.FromDateTime(dateFrom.Value))
                        .ToList();
                }

                if (dateTo.HasValue)
                {
                    receivingReports = receivingReports
                        .Where(s => s.Date <= DateOnly.FromDateTime(dateTo.Value))
                        .ToList();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    receivingReports = receivingReports
                        .Where(s =>
                            s.ReceivingReportNo!.ToLower().Contains(searchValue) ||
                            s.Date.ToString(SD.Date_Format).ToLower().Contains(searchValue) ||
                            s.PurchaseOrder!.PurchaseOrderNo!.ToLower().Contains(searchValue) ||
                            s.QuantityDelivered.ToString().Contains(searchValue) ||
                            s.QuantityReceived.ToString().Contains(searchValue) ||
                            s.CreatedBy!.ToLower().Contains(searchValue) ||
                            s.Status.ToLower().Contains(searchValue)
                        )
                        .ToList();
                }

                // Apply sorting if provided
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Name;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";

                    receivingReports = receivingReports
                        .AsQueryable()
                        .OrderBy($"{columnName} {sortDirection}")
                        .ToList();
                }

                var totalRecords = receivingReports.Count();

                // Apply pagination - HANDLE -1 FOR "ALL"
                IEnumerable<FilprideReceivingReport> pagedReceivingReports;

                if (parameters.Length == -1)
                {
                    // "All" selected - return all records
                    pagedReceivingReports = receivingReports;
                }
                else
                {
                    // Normal pagination
                    pagedReceivingReports = receivingReports
                        .Skip(parameters.Start)
                        .Take(parameters.Length);
                }

                var pagedData = pagedReceivingReports
                    .Select(x => new
                    {
                        x.ReceivingReportId,
                        x.ReceivingReportNo,
                        x.Date,
                        purchaseOrderNo = x.PurchaseOrder!.PurchaseOrderNo,
                        x.QuantityDelivered,
                        x.QuantityReceived,
                        x.CreatedBy,
                        x.Status,
                        // Include status flags for badge rendering
                        isPosted = x.PostedBy != null,
                        isVoided = x.VoidedBy != null,
                        isCanceled = x.CanceledBy != null
                    })
                    .ToList();

                return Json(new
                {
                    draw = parameters.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get receiving reports. Error: {ErrorMessage}, Stack: {StackTrace}.",
                    ex.Message, ex.StackTrace);
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        //Download as .xlsx file.(Export)

        #region -- export xlsx record --

        [HttpPost]
        public async Task<IActionResult> Export(string selectedRecord)
        {
            if (string.IsNullOrEmpty(selectedRecord))
            {
                // Handle the case where no invoices are selected
                return RedirectToAction(nameof(Index), new { filterType = await GetCurrentFilterType() });
            }

            var recordIds = selectedRecord.Split(',').Select(int.Parse).ToList();

            // Retrieve the selected records from the database
            var selectedList = await _dbContext.FilprideReceivingReports
                .Where(rr => recordIds.Contains(rr.ReceivingReportId))
                .Include(rr => rr.PurchaseOrder)
                .OrderBy(rr => rr.ReceivingReportNo)
                .ToListAsync();

            // Create the Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the Excel package

                #region -- Purchase Order Table Header --

                var worksheet2 = package.Workbook.Worksheets.Add("PurchaseOrder");

                worksheet2.Cells["A1"].Value = "Date";
                worksheet2.Cells["B1"].Value = "Terms";
                worksheet2.Cells["C1"].Value = "Quantity";
                worksheet2.Cells["D1"].Value = "Price";
                worksheet2.Cells["E1"].Value = "Amount";
                worksheet2.Cells["F1"].Value = "FinalPrice";
                worksheet2.Cells["G1"].Value = "QuantityReceived";
                worksheet2.Cells["H1"].Value = "IsReceived";
                worksheet2.Cells["I1"].Value = "ReceivedDate";
                worksheet2.Cells["J1"].Value = "Remarks";
                worksheet2.Cells["K1"].Value = "CreatedBy";
                worksheet2.Cells["L1"].Value = "CreatedDate";
                worksheet2.Cells["M1"].Value = "IsClosed";
                worksheet2.Cells["N1"].Value = "CancellationRemarks";
                worksheet2.Cells["O1"].Value = "OriginalProductId";
                worksheet2.Cells["P1"].Value = "OriginalSeriesNumber";
                worksheet2.Cells["Q1"].Value = "OriginalSupplierId";
                worksheet2.Cells["R1"].Value = "OriginalDocumentId";
                worksheet2.Cells["S1"].Value = "PostedBy";
                worksheet2.Cells["T1"].Value = "PostedDate";
                worksheet2.Cells["U1"].Value = "EditedBy";
                worksheet2.Cells["V1"].Value = "EditedDate";
                worksheet2.Cells["W1"].Value = "CanceledBy";
                worksheet2.Cells["X1"].Value = "CanceledDate";
                worksheet2.Cells["Y1"].Value = "VoidedBy";
                worksheet2.Cells["Z1"].Value = "VoidedDate";
                worksheet2.Cells["AA1"].Value = "SupplierName";
                worksheet2.Cells["AB1"].Value = "SupplierAddress";
                worksheet2.Cells["AC1"].Value = "SupplierTin";
                worksheet2.Cells["AD1"].Value = "ProductName";
                worksheet2.Cells["AE1"].Value = "SupplierSalesOrderNo";
                worksheet2.Cells["AF1"].Value = "Status";
                worksheet2.Cells["AG1"].Value = "IsSubPo";
                worksheet2.Cells["AH1"].Value = "SubPoSeries";
                worksheet2.Cells["AI1"].Value = "CustomerId";
                worksheet2.Cells["AJ1"].Value = "OldPoNo";
                worksheet2.Cells["AK1"].Value = "TriggerDate";
                worksheet2.Cells["AL1"].Value = "UnTriggeredQuantity";
                worksheet2.Cells["AM1"].Value = "PickUpPointId";
                worksheet2.Cells["AN1"].Value = "VatType";
                worksheet2.Cells["AO1"].Value = "TaxType";
                worksheet2.Cells["AP1"].Value = "TypeOfPurchase";

                #endregion -- Purchase Order Table Header --

                #region -- Receving Report Table Header --

                var worksheet = package.Workbook.Worksheets.Add("ReceivingReport");

                worksheet.Cells["A1"].Value = "Date";
                worksheet.Cells["B1"].Value = "DueDate";
                worksheet.Cells["C1"].Value = "SupplierInvoiceNumber";
                worksheet.Cells["D1"].Value = "SupplierInvoiceDate";
                worksheet.Cells["E1"].Value = "TruckOrVessels";
                worksheet.Cells["F1"].Value = "QuantityDelivered";
                worksheet.Cells["G1"].Value = "QuantityReceived";
                worksheet.Cells["H1"].Value = "GainOrLoss";
                worksheet.Cells["I1"].Value = "Amount";
                worksheet.Cells["J1"].Value = "Net of Vat";
                worksheet.Cells["K1"].Value = "Vat";
                worksheet.Cells["L1"].Value = "WHT";
                worksheet.Cells["M1"].Value = "OtherRef";
                worksheet.Cells["N1"].Value = "Remarks";
                worksheet.Cells["O1"].Value = "AmountPaid";
                worksheet.Cells["P1"].Value = "IsPaid";
                worksheet.Cells["Q1"].Value = "PaidDate";
                worksheet.Cells["R1"].Value = "CanceledQuantity";
                worksheet.Cells["S1"].Value = "CreatedBy";
                worksheet.Cells["T1"].Value = "CreatedDate";
                worksheet.Cells["U1"].Value = "CancellationRemarks";
                worksheet.Cells["V1"].Value = "ReceivedDate";
                worksheet.Cells["W1"].Value = "OriginalPOId";
                worksheet.Cells["X1"].Value = "OriginalSeriesNumber";
                worksheet.Cells["Y1"].Value = "OriginalDocumentId";
                worksheet.Cells["Z1"].Value = "PostedBy";
                worksheet.Cells["AA1"].Value = "PostedDate";
                worksheet.Cells["AB1"].Value = "EditedBy";
                worksheet.Cells["AC1"].Value = "EditedDate";
                worksheet.Cells["AD1"].Value = "CanceledBy";
                worksheet.Cells["AE1"].Value = "CanceledDate";
                worksheet.Cells["AF1"].Value = "VoidedBy";
                worksheet.Cells["AG1"].Value = "VoidedDate";
                worksheet.Cells["AH1"].Value = "Supplier Name";
                worksheet.Cells["AI1"].Value = "SupplierDrNo";
                worksheet.Cells["AJ1"].Value = "WithdrawalCertificate";
                worksheet.Cells["AK1"].Value = "AuthorityToLoadNo";
                worksheet.Cells["AL1"].Value = "DeliveryReceiptId";
                worksheet.Cells["AM1"].Value = "Status";
                worksheet.Cells["AN1"].Value = "IsCostUpdated";
                worksheet.Cells["AO1"].Value = "OldRRNo";
                worksheet.Cells["AP1"].Value = "CostBasedOnSoa";
                worksheet.Cells["AQ1"].Value = "TaxPercentage";

                #endregion -- Receving Report Table Header --

                #region -- Receving Report Export --

                int row = 2;

                foreach (var item in selectedList)
                {
                    var vatableAmount = item.PurchaseOrder?.VatType == SD.VatType_Vatable ? item.Amount / 1.12m : item.Amount;
                    var vatAmount = item.PurchaseOrder?.VatType == SD.VatType_Vatable ? item.Amount - vatableAmount : 0m;
                    var taxAmount = item.PurchaseOrder?.TaxType == SD.TaxType_WithTax ? vatableAmount * item.TaxPercentage : 0m;
                    worksheet.Cells[row, 1].Value = item.Date.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 2].Value = item.DueDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 3].Value = item.SupplierInvoiceNumber;
                    worksheet.Cells[row, 4].Value = item.SupplierInvoiceDate?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 5].Value = item.TruckOrVessels;
                    worksheet.Cells[row, 6].Value = item.QuantityDelivered;
                    worksheet.Cells[row, 7].Value = item.QuantityReceived;
                    worksheet.Cells[row, 8].Value = item.GainOrLoss;
                    worksheet.Cells[row, 9].Value = item.Amount;
                    worksheet.Cells[row, 10].Value = vatableAmount;
                    worksheet.Cells[row, 11].Value = vatAmount;
                    worksheet.Cells[row, 12].Value = taxAmount;
                    worksheet.Cells[row, 13].Value = item.AuthorityToLoadNo;
                    worksheet.Cells[row, 14].Value = item.Remarks;
                    worksheet.Cells[row, 15].Value = item.AmountPaid;
                    worksheet.Cells[row, 16].Value = item.IsPaid;
                    worksheet.Cells[row, 17].Value = item.PaidDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                    worksheet.Cells[row, 18].Value = item.CanceledQuantity;
                    worksheet.Cells[row, 19].Value = item.CreatedBy;
                    worksheet.Cells[row, 20].Value = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                    worksheet.Cells[row, 21].Value = item.CancellationRemarks;
                    worksheet.Cells[row, 22].Value = item.ReceivedDate?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 23].Value = item.POId;
                    worksheet.Cells[row, 24].Value = item.ReceivingReportNo;
                    worksheet.Cells[row, 25].Value = item.ReceivingReportId;
                    worksheet.Cells[row, 26].Value = item.PostedBy;
                    worksheet.Cells[row, 27].Value = item.PostedDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") ?? null;
                    worksheet.Cells[row, 28].Value = item.EditedBy;
                    worksheet.Cells[row, 29].Value = item.EditedDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") ?? null;
                    worksheet.Cells[row, 30].Value = item.CanceledBy;
                    worksheet.Cells[row, 31].Value = item.CanceledDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") ?? null;
                    worksheet.Cells[row, 32].Value = item.VoidedBy;
                    worksheet.Cells[row, 33].Value = item.VoidedDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") ?? null;
                    worksheet.Cells[row, 34].Value = item.PurchaseOrder?.SupplierName;
                    worksheet.Cells[row, 35].Value = item.SupplierDrNo;
                    worksheet.Cells[row, 36].Value = item.WithdrawalCertificate;
                    worksheet.Cells[row, 37].Value = item.AuthorityToLoadNo;
                    worksheet.Cells[row, 38].Value = item.DeliveryReceiptId;
                    worksheet.Cells[row, 39].Value = item.Status;
                    worksheet.Cells[row, 40].Value = item.IsCostUpdated;
                    worksheet.Cells[row, 41].Value = item.OldRRNo;
                    worksheet.Cells[row, 42].Value = item.CostBasedOnSoa;
                    worksheet.Cells[row, 43].Value = item.TaxPercentage;

                    row++;
                }

                #endregion -- Receving Report Export --

                #region -- Purchase Order Export --

                int poRow = 2;
                var currentPo = "";

                foreach (var item in selectedList.DistinctBy(rr => rr.PurchaseOrder!.PurchaseOrderNo))
                {
                    if (item.PurchaseOrder == null)
                    {
                        continue;
                    }

                    if (item.PurchaseOrder.PurchaseOrderNo == currentPo)
                    {
                        continue;
                    }

                    currentPo = item.PurchaseOrder.PurchaseOrderNo;
                    worksheet2.Cells[poRow, 1].Value = item.PurchaseOrder.Date.ToString("yyyy-MM-dd");
                    worksheet2.Cells[poRow, 2].Value = item.PurchaseOrder.Terms;
                    worksheet2.Cells[poRow, 3].Value = item.PurchaseOrder.Quantity;
                    worksheet2.Cells[poRow, 4].Value = await _unitOfWork.FilpridePurchaseOrder.GetPurchaseOrderCost(item.PurchaseOrder.PurchaseOrderId);
                    worksheet2.Cells[poRow, 5].Value = item.PurchaseOrder.Amount;
                    worksheet2.Cells[poRow, 6].Value = item.PurchaseOrder.FinalPrice;
                    worksheet2.Cells[poRow, 7].Value = item.PurchaseOrder.QuantityReceived;
                    worksheet2.Cells[poRow, 8].Value = item.PurchaseOrder.IsReceived;
                    worksheet2.Cells[poRow, 9].Value = item.PurchaseOrder.ReceivedDate != default ? item.PurchaseOrder.ReceivedDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff zzz") : null;
                    worksheet2.Cells[poRow, 10].Value = item.PurchaseOrder.Remarks;
                    worksheet2.Cells[poRow, 11].Value = item.PurchaseOrder.CreatedBy;
                    worksheet2.Cells[poRow, 12].Value = item.PurchaseOrder.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                    worksheet2.Cells[poRow, 13].Value = item.PurchaseOrder.IsClosed;
                    worksheet2.Cells[poRow, 14].Value = item.PurchaseOrder.CancellationRemarks;
                    worksheet2.Cells[poRow, 15].Value = item.PurchaseOrder.ProductId;
                    worksheet2.Cells[poRow, 16].Value = item.PurchaseOrder.PurchaseOrderNo;
                    worksheet2.Cells[poRow, 17].Value = item.PurchaseOrder.SupplierId;
                    worksheet2.Cells[poRow, 18].Value = item.PurchaseOrder.PurchaseOrderId;
                    worksheet2.Cells[poRow, 19].Value = item.PurchaseOrder.PostedBy;
                    worksheet2.Cells[poRow, 20].Value = item.PurchaseOrder.PostedDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") ?? null;
                    worksheet2.Cells[poRow, 21].Value = item.PurchaseOrder.EditedBy;
                    worksheet2.Cells[poRow, 22].Value = item.PurchaseOrder.EditedDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") ?? null;
                    worksheet2.Cells[poRow, 23].Value = item.PurchaseOrder.CanceledBy;
                    worksheet2.Cells[poRow, 24].Value = item.PurchaseOrder.CanceledDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") ?? null;
                    worksheet2.Cells[poRow, 25].Value = item.PurchaseOrder.VoidedBy;
                    worksheet2.Cells[poRow, 26].Value = item.PurchaseOrder.VoidedDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") ?? null;
                    worksheet2.Cells[poRow, 27].Value = item.PurchaseOrder.SupplierName;
                    worksheet2.Cells[poRow, 28].Value = item.PurchaseOrder.SupplierAddress;
                    worksheet2.Cells[poRow, 29].Value = item.PurchaseOrder.SupplierTin;
                    worksheet2.Cells[poRow, 30].Value = item.PurchaseOrder.ProductName;
                    worksheet2.Cells[poRow, 31].Value = item.PurchaseOrder.SupplierSalesOrderNo;
                    worksheet2.Cells[poRow, 32].Value = item.PurchaseOrder.Status;
                    worksheet2.Cells[poRow, 33].Value = item.PurchaseOrder.IsSubPo;
                    worksheet2.Cells[poRow, 34].Value = item.PurchaseOrder.SubPoSeries;
                    worksheet2.Cells[poRow, 35].Value = item.PurchaseOrder.CustomerId;
                    worksheet2.Cells[poRow, 36].Value = item.PurchaseOrder.OldPoNo;
                    worksheet2.Cells[poRow, 37].Value = item.PurchaseOrder.TriggerDate.ToString("yyyy-MM-dd");
                    worksheet2.Cells[poRow, 38].Value = item.PurchaseOrder.UnTriggeredQuantity;
                    worksheet2.Cells[poRow, 39].Value = item.PurchaseOrder.PickUpPointId;
                    worksheet2.Cells[poRow, 40].Value = item.PurchaseOrder.VatType;
                    worksheet2.Cells[poRow, 41].Value = item.PurchaseOrder.TaxType;
                    worksheet2.Cells[poRow, 42].Value = item.PurchaseOrder.TypeOfPurchase;

                    poRow++;
                }

                #endregion -- Purchase Order Export --

                // Convert the Excel package to a byte array
                var excelBytes = await package.GetAsByteArrayAsync();

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"ReceivingReportList_IBS_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx");
            }
        }

        #endregion -- export xlsx record --

        [HttpGet]
        public IActionResult GetAllReceivingReportIds()
        {
            var rrIds = _dbContext.FilprideReceivingReports
                                     .Where(rr => rr.Type == nameof(DocumentType.Documented))
                                     .Select(rr => rr.ReceivingReportId)
                                     .ToList();

            return Json(rrIds);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReJournalPurchase(int? month, int? year, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (!month.HasValue || !year.HasValue)
                {
                    return BadRequest("Month and year are required.");
                }

                var receivingReports = await _unitOfWork.FilprideReceivingReport
                    .GetAllAsync(x =>

                        x.Status == nameof(Status.Posted) &&
                        x.Date.Month == month &&
                        x.Date.Year == year,
                        cancellationToken);

                if (!receivingReports.Any())
                {
                    return Json(new { sucess = true, message = "No records were returned." });
                }

                var rrReferences = receivingReports
                    .Select(x => x.ReceivingReportNo!)
                    .Distinct()
                    .ToList();

                var existingGlEntries = await _dbContext.FilprideGeneralLedgerBooks
                    .Where(x => rrReferences.Contains(x.Reference))
                    .ToListAsync(cancellationToken);

                if (existingGlEntries.Count != 0)
                {
                    _dbContext.FilprideGeneralLedgerBooks.RemoveRange(existingGlEntries);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                foreach (var receivingReport in receivingReports
                             .OrderBy(x => x.Date))
                {
                    await _unitOfWork.FilprideReceivingReport
                        .PostAsync(receivingReport, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return Json(new { month, year, count = receivingReports.Count() });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
