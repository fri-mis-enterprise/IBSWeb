using IBS.DataAccess.Data;
using IBS.Models;
using IBS.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IBSWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles = "Admin")]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(ApplicationDbContext dbContext, ILogger<MaintenanceController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var isEnabled = await _dbContext.AppSettings
                .AsNoTracking()
                .Where(setting => setting.SettingKey == AppSettingKey.MaintenanceMode)
                .Select(setting => setting.Value == "true")
                .SingleOrDefaultAsync(cancellationToken);

            return View(isEnabled);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(bool isEnabled, CancellationToken cancellationToken)
        {
            try
            {
                var setting = await _dbContext.AppSettings
                    .SingleOrDefaultAsync(
                        setting => setting.SettingKey == AppSettingKey.MaintenanceMode,
                        cancellationToken);

                if (setting == null)
                {
                    setting = new AppSetting
                    {
                        SettingKey = AppSettingKey.MaintenanceMode,
                        Value = isEnabled ? "true" : "false"
                    };
                    _dbContext.AppSettings.Add(setting);
                }
                else
                {
                    setting.Value = isEnabled ? "true" : "false";
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Maintenance mode was {MaintenanceState} by {UserName}",
                    isEnabled ? "enabled" : "disabled",
                    User.Identity?.Name);
                TempData["success"] = $"Maintenance mode has been {(isEnabled ? "enabled" : "disabled")}.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update maintenance mode");
                TempData["error"] = "Maintenance mode could not be updated. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
