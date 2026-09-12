using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Filpride.ViewModels;
using IBS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBSWeb.Areas.Filpride.Controllers
{
    [Area("Filpride")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class CollectionCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ProvisionalReceiptTaggingService _tagging;
        private readonly ILogger<CollectionCategoryController> _logger;

        public CollectionCategoryController(
            ApplicationDbContext db,
            IUnitOfWork unitOfWork,
            ProvisionalReceiptTaggingService tagging,
            ILogger<CollectionCategoryController> logger)
        {
            _db = db;
            _unitOfWork = unitOfWork;
            _tagging = tagging;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var categories = await _db.FilprideCollectionCategories
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(c => c.CreditAccount)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            if (id == 0)
            {
                var newCategory = new CollectionCategoryViewModel();
                await PopulateCreditAccountsAsync(newCategory, cancellationToken);
                return View(newCategory);
            }

            var category = await _db.FilprideCollectionCategories
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (category == null)
            {
                return NotFound();
            }

            var isUsed = await _db.FilprideProvisionalReceipts
                .AnyAsync(p => p.CollectionCategoryId == id, cancellationToken);
            var form = new CollectionCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                CreditAccountId = category.CreditAccountId,
                TaggingRequirement = category.TaggingRequirement,
                AllowCompany = category.AllowCompany,
                AllowEmployee = category.AllowEmployee,
                AllowBankAccount = category.AllowBankAccount,
                IsActive = category.IsActive,
                IsUsed = isUsed
            };
            await PopulateCreditAccountsAsync(form, cancellationToken);
            return View(form);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CollectionCategoryViewModel form, CancellationToken cancellationToken)
        {
            form.IsUsed = form.Id != 0 && await _db.FilprideProvisionalReceipts
                .AnyAsync(p => p.CollectionCategoryId == form.Id, cancellationToken);
            if (!ModelState.IsValid)
            {
                await PopulateCreditAccountsAsync(form, cancellationToken);
                return View(form);
            }

            try
            {
                var user = User.FindFirstValue(ClaimTypes.GivenName) ?? User.Identity?.Name ?? string.Empty;
                await _tagging.SaveCategoryAsync(form, user, cancellationToken);
                TempData["success"] = "Collection category saved.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save collection category {CategoryId}", form.Id);
                ModelState.AddModelError(string.Empty, "The category could not be saved. Refresh and retry; its name may already be in use.");
            }
            await PopulateCreditAccountsAsync(form, cancellationToken);
            return View(form);
        }

        private async Task PopulateCreditAccountsAsync(CollectionCategoryViewModel form, CancellationToken cancellationToken)
        {
            form.CreditAccounts = await _unitOfWork.GetChartOfAccountListAsyncById(cancellationToken);
        }
    }
}
