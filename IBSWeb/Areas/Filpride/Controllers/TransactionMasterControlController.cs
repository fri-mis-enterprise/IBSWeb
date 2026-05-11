using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models;
using IBS.Models.Filpride.Books;
using IBS.Models.Filpride.ViewModels;
using IBS.Services.Attributes;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using IBS.Models.Enums;

namespace IBSWeb.Areas.Filpride.Controllers
{
    [Area(nameof(Filpride))]
    [CompanyAuthorize(nameof(Filpride))]
    [DepartmentAuthorize(SD.Department_Accounting, SD.Department_ManagementAccounting)]
    public class TransactionMasterControlController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TransactionMasterControlController> _logger;

        public TransactionMasterControlController(
            ApplicationDbContext dbContext,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<TransactionMasterControlController> logger)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        private string GetUserFullName()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                   ?? User.Identity?.Name!;
        }

        private async Task<string?> GetCompanyClaimAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.FirstOrDefault(c => c.Type == "Company")?.Value;
        }

        public IActionResult Index()
        {
            return View(new TransactionMasterControlViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(TransactionMasterControlViewModel searchModel, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(searchModel.ReferenceNo))
            {
                ModelState.AddModelError(nameof(searchModel.ReferenceNo), "Reference number is required.");
                return View(searchModel);
            }

            var company = await GetCompanyClaimAsync();
            var referenceNo = searchModel.ReferenceNo.Trim();

            // Try to find in CV
            var cvHeader = await _dbContext.FilprideCheckVoucherHeaders
                .FirstOrDefaultAsync(x => x.CheckVoucherHeaderNo == referenceNo && x.Company == company, cancellationToken);

            if (cvHeader != null)
            {
                return RedirectToAction(nameof(Edit), new { referenceNo = referenceNo, type = "CV" });
            }

            // Try to find in JV
            var jvHeader = await _dbContext.FilprideJournalVoucherHeaders
                .FirstOrDefaultAsync(x => x.JournalVoucherHeaderNo == referenceNo && x.Company == company, cancellationToken);

            if (jvHeader != null)
            {
                return RedirectToAction(nameof(Edit), new { referenceNo = referenceNo, type = "JV" });
            }

            // Try to find in SI
            var siHeader = await _dbContext.FilprideSalesInvoices
                .FirstOrDefaultAsync(x => x.SalesInvoiceNo == referenceNo && x.Company == company, cancellationToken);

            if (siHeader != null)
            {
                return RedirectToAction(nameof(Edit), new { referenceNo = referenceNo, type = "SI" });
            }

            // Try to find in SV
            var svHeader = await _dbContext.FilprideServiceInvoices
                .FirstOrDefaultAsync(x => x.ServiceInvoiceNo == referenceNo && x.Company == company, cancellationToken);

            if (svHeader != null)
            {
                return RedirectToAction(nameof(Edit), new { referenceNo = referenceNo, type = "SV" });
            }

            // Try to find in CR
            var crHeader = await _dbContext.FilprideCollectionReceipts
                .FirstOrDefaultAsync(x => x.CollectionReceiptNo == referenceNo && x.Company == company, cancellationToken);

            if (crHeader != null)
            {
                return RedirectToAction(nameof(Edit), new { referenceNo = referenceNo, type = "CR" });
            }

            TempData["error"] = "No transaction found with that reference number.";
            return View(searchModel);
        }

        public async Task<IActionResult> Edit(string referenceNo, string type, CancellationToken cancellationToken)
        {
            var company = await GetCompanyClaimAsync();
            TransactionMasterControlViewModel model = new() { ReferenceNo = referenceNo, TransactionType = type };

            if (type == "CV")
            {
                var header = await _dbContext.FilprideCheckVoucherHeaders
                    .FirstOrDefaultAsync(x => x.CheckVoucherHeaderNo == referenceNo && x.Company == company, cancellationToken);

                if (header == null) return NotFound();

                model.Date = header.Date;

                var particulars = header.Particulars ?? string.Empty;
                var index = particulars.IndexOf(". Payment for ", StringComparison.Ordinal);
                if (index >= 0)
                {
                    model.Particulars = particulars.Substring(0, index).Trim();
                    model.PaymentFor = particulars.Substring(index + 14).Trim();
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
                var header = await _dbContext.FilprideJournalVoucherHeaders
                    .FirstOrDefaultAsync(x => x.JournalVoucherHeaderNo == referenceNo && x.Company == company, cancellationToken);

                if (header == null) return NotFound();

                model.Date = header.Date;
                model.Particulars = header.Particulars;
                model.IsFound = true;
            }
            else if (type == "SI")
            {
                var header = await _dbContext.FilprideSalesInvoices
                    .FirstOrDefaultAsync(x => x.SalesInvoiceNo == referenceNo && x.Company == company, cancellationToken);

                if (header == null) return NotFound();

                model.Date = header.TransactionDate;
                model.Particulars = header.Remarks;
                model.IsFound = true;
            }
            else if (type == "SV")
            {
                var header = await _dbContext.FilprideServiceInvoices
                    .FirstOrDefaultAsync(x => x.ServiceInvoiceNo == referenceNo && x.Company == company, cancellationToken);

                if (header == null) return NotFound();

                model.Date = header.Period;
                model.Particulars = header.Instructions;
                model.IsFound = true;
            }
            else if (type == "CR")
            {
                var header = await _dbContext.FilprideCollectionReceipts
                    .FirstOrDefaultAsync(x => x.CollectionReceiptNo == referenceNo && x.Company == company, cancellationToken);

                if (header == null) return NotFound();

                model.Date = header.TransactionDate;
                model.Particulars = header.Remarks ?? string.Empty;
                model.IsFound = true;
            }
            else
            {
                return BadRequest("Invalid transaction type.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TransactionMasterControlViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var company = await GetCompanyClaimAsync();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (model.TransactionType == "CV")
                {
                    var header = await _dbContext.FilprideCheckVoucherHeaders
                        .FirstOrDefaultAsync(x => x.CheckVoucherHeaderNo == model.ReferenceNo && x.Company == company, cancellationToken);

                    if (header == null) throw new Exception("CV Header not found.");

                    var finalParticulars = !string.IsNullOrWhiteSpace(model.PaymentFor)
                        ? $"{model.Particulars}. Payment for {model.PaymentFor}"
                        : model.Particulars;

                    // Update Header
                    header.Particulars = finalParticulars;
                    header.Payee = model.Payee;
                    header.CheckNo = model.CheckNo;
                    header.CheckDate = model.CheckDate;
                    header.EditedBy = GetUserFullName();
                    header.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                    // Update Disbursement Books
                    var disbursementBooks = await _dbContext.FilprideDisbursementBooks
                        .Where(x => x.CVNo == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var book in disbursementBooks)
                    {
                        book.Particulars = finalParticulars;
                        book.Payee = model.Payee ?? book.Payee;
                        book.CheckNo = model.CheckNo ?? book.CheckNo;
                        book.CheckDate = model.CheckDate?.ToString("MM/dd/yyyy") ?? book.CheckDate;
                    }

                    // Update GL Books
                    var glBooks = await _dbContext.FilprideGeneralLedgerBooks
                        .Where(x => x.Reference == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var gl in glBooks)
                    {
                        gl.Description = finalParticulars;
                    }

                    // Cascading update: If this is an Invoicing CV, update the Payment CV that pays for it.
                    if (header.CvType == nameof(CVType.Invoicing))
                    {
                        var paymentCvIds = await _dbContext.FilprideMultipleCheckVoucherPayments
                            .Where(x => x.CheckVoucherHeaderInvoiceId == header.CheckVoucherHeaderId)
                            .Select(x => x.CheckVoucherHeaderPaymentId)
                            .ToListAsync(cancellationToken);

                        foreach (var paymentId in paymentCvIds)
                        {
                            var paymentHeader = await _dbContext.FilprideCheckVoucherHeaders
                                .FirstOrDefaultAsync(x => x.CheckVoucherHeaderId == paymentId, cancellationToken);

                            if (paymentHeader != null)
                            {
                                var oldPaymentParticulars = paymentHeader.Particulars ?? "";
                                var paymentIndex = oldPaymentParticulars.IndexOf(". Payment for ", StringComparison.Ordinal);

                                if (paymentIndex >= 0)
                                {
                                    var suffix = oldPaymentParticulars.Substring(paymentIndex);
                                    var newPaymentParticulars = model.Particulars + suffix;

                                    if (paymentHeader.Particulars != newPaymentParticulars)
                                    {
                                        paymentHeader.Particulars = newPaymentParticulars;
                                        paymentHeader.EditedBy = GetUserFullName();
                                        paymentHeader.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                                        var pBooks = await _dbContext.FilprideDisbursementBooks
                                            .Where(x => x.CVNo == paymentHeader.CheckVoucherHeaderNo && x.Company == company)
                                            .ToListAsync(cancellationToken);
                                        foreach (var b in pBooks) b.Particulars = newPaymentParticulars;

                                        var pGls = await _dbContext.FilprideGeneralLedgerBooks
                                            .Where(x => x.Reference == paymentHeader.CheckVoucherHeaderNo && x.Company == company)
                                            .ToListAsync(cancellationToken);
                                        foreach (var gl in pGls) gl.Description = newPaymentParticulars;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (model.TransactionType == "JV")
                {
                    var header = await _dbContext.FilprideJournalVoucherHeaders
                        .FirstOrDefaultAsync(x => x.JournalVoucherHeaderNo == model.ReferenceNo && x.Company == company, cancellationToken);

                    if (header == null) throw new Exception("JV Header not found.");

                    // Update Header
                    header.Particulars = model.Particulars;
                    header.EditedBy = GetUserFullName();
                    header.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                    // Update Journal Books
                    var journalBooks = await _dbContext.FilprideJournalBooks
                        .Where(x => x.Reference == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var book in journalBooks)
                    {
                        book.Description = model.Particulars;
                    }

                    // Update GL Books
                    var glBooks = await _dbContext.FilprideGeneralLedgerBooks
                        .Where(x => x.Reference == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var gl in glBooks)
                    {
                        gl.Description = model.Particulars;
                    }
                }
                else if (model.TransactionType == "SI")
                {
                    var header = await _dbContext.FilprideSalesInvoices
                        .FirstOrDefaultAsync(x => x.SalesInvoiceNo == model.ReferenceNo && x.Company == company, cancellationToken);

                    if (header == null) throw new Exception("SI Header not found.");

                    header.Remarks = model.Particulars;
                    header.EditedBy = GetUserFullName();
                    header.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                    var salesBooks = await _dbContext.FilprideSalesBooks
                        .Where(x => x.SerialNo == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var book in salesBooks)
                    {
                        book.Description = model.Particulars;
                    }

                    var glBooks = await _dbContext.FilprideGeneralLedgerBooks
                        .Where(x => x.Reference == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var gl in glBooks)
                    {
                        gl.Description = model.Particulars;
                    }
                }
                else if (model.TransactionType == "SV")
                {
                    var header = await _dbContext.FilprideServiceInvoices
                        .FirstOrDefaultAsync(x => x.ServiceInvoiceNo == model.ReferenceNo && x.Company == company, cancellationToken);

                    if (header == null) throw new Exception("SV Header not found.");

                    header.Instructions = model.Particulars;
                    header.EditedBy = GetUserFullName();
                    header.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                    var salesBooks = await _dbContext.FilprideSalesBooks
                        .Where(x => x.SerialNo == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var book in salesBooks)
                    {
                        book.Description = model.Particulars;
                    }

                    var glBooks = await _dbContext.FilprideGeneralLedgerBooks
                        .Where(x => x.Reference == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var gl in glBooks)
                    {
                        gl.Description = model.Particulars;
                    }
                }
                else if (model.TransactionType == "CR")
                {
                    var header = await _dbContext.FilprideCollectionReceipts
                        .FirstOrDefaultAsync(x => x.CollectionReceiptNo == model.ReferenceNo && x.Company == company, cancellationToken);

                    if (header == null) throw new Exception("CR Header not found.");

                    header.Remarks = model.Particulars;
                    header.EditedBy = GetUserFullName();
                    header.EditedDate = DateTimeHelper.GetCurrentPhilippineTime();

                    var cashReceiptBooks = await _dbContext.FilprideCashReceiptBooks
                        .Where(x => x.RefNo == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var book in cashReceiptBooks)
                    {
                        book.Particulars = model.Particulars;
                    }

                    var glBooks = await _dbContext.FilprideGeneralLedgerBooks
                        .Where(x => x.Reference == model.ReferenceNo && x.Company == company)
                        .ToListAsync(cancellationToken);

                    foreach (var gl in glBooks)
                    {
                        gl.Description = model.Particulars;
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                // Audit Trail
                FilprideAuditTrail auditTrail = new(
                    GetUserFullName(),
                    $"Updated particulars/metadata for {model.TransactionType}# {model.ReferenceNo} via Master Control",
                    "Master Control",
                    company!
                );
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrail, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                TempData["success"] = "Transaction updated successfully across all records.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error updating transaction via Master Control. Ref: {Ref}", model.ReferenceNo);
                TempData["error"] = $"Error: {ex.Message}";
                return View(model);
            }
        }
    }
}
