using System.Security.Claims;
using IBS.DTOs;
using IBS.Services;
using IBSWeb.Areas.Filpride.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace IBSWeb.Areas.Filpride.Controllers
{
    [Area(nameof(Filpride))]
    [Authorize]
    public class GlImportController : Controller
    {
        private readonly GlImportService _service;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GlImportController> _logger;

        public GlImportController(GlImportService service, IMemoryCache cache, ILogger<GlImportController> logger)
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return View(new GlImportViewModel { History = await _service.GetHistoryAsync(cancellationToken) });
        }

        [HttpGet]
        public async Task<IActionResult> Template(CancellationToken cancellationToken)
        {
            var contents = await _service.CreateTemplateAsync(cancellationToken);
            return File(contents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "gl-template.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(GlImportService.MaximumFileBytes + (1024 * 1024))]
        [RequestFormLimits(MultipartBodyLengthLimit = GlImportService.MaximumFileBytes + (1024 * 1024))]
        public async Task<IActionResult> Preview(IFormFile? file, CancellationToken cancellationToken)
        {
            var model = new GlImportViewModel { History = await _service.GetHistoryAsync(cancellationToken) };
            if (file == null || file.Length == 0 || file.Length > GlImportService.MaximumFileBytes)
            {
                ModelState.AddModelError(string.Empty, "Select a nonempty CSV file no larger than 10 MB.");
                return View("Index", model);
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            byte[] contents = stream.ToArray();
            model.Preview = await _service.PreviewAsync(contents, file.FileName, cancellationToken);
            if (model.Preview.CanImport)
            {
                string token = Guid.NewGuid().ToString("N");
                _cache.Set(GetCacheKey(token), (contents, model.Preview.FileName), new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                    Size = contents.Length + 1024
                });
                model.PreviewToken = token;
            }
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(string? previewToken, CancellationToken cancellationToken)
        {
            if (!Guid.TryParseExact(previewToken, "N", out _) ||
                !_cache.TryGetValue(GetCacheKey(previewToken!), out (byte[] Contents, string FileName) pending))
            {
                TempData["error"] = "The preview expired or is unavailable. Upload the file again.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                Guid batchId = await _service.ImportAsync(pending.Contents, pending.FileName, User.Identity!.Name!, cancellationToken);
                _cache.Remove(GetCacheKey(previewToken!));
                TempData["success"] = $"GL batch {batchId} imported successfully.";
            }
            catch (InvalidOperationException ex)
            {
                GlImportPreview preview = await _service.PreviewAsync(pending.Contents, pending.FileName, cancellationToken);
                if (preview.Errors.Count == 0)
                {
                    preview.Errors.Add(ex.Message);
                }
                return View("Index", new GlImportViewModel
                {
                    Preview = preview,
                    History = await _service.GetHistoryAsync(cancellationToken)
                });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to import GL for {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["error"] = "The GL import failed. Upload the file again or check the server log.";
            }
            return RedirectToAction(nameof(Index));
        }

        private string GetCacheKey(string token)
        {
            return $"gl-import:{User.FindFirstValue(ClaimTypes.NameIdentifier)}:{token}";
        }
    }
}
