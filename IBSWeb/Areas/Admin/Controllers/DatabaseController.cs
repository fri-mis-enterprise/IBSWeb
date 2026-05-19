using IBS.Services;
using IBSWeb.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace IBSWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DatabaseController : Controller
    {
        private readonly IDbSyncService _dbSyncService;

        public DatabaseController(IDbSyncService dbSyncService)
        {
            _dbSyncService = dbSyncService;
        }

        public IActionResult Index()
        {
            return View(new DbSyncViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sync(DbSyncViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please provide all required connection details.";
                return View("Index", model);
            }

            try
            {
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = model.Server,
                    Port = model.Port,
                    Database = model.Database,
                    Username = model.Username,
                    Password = model.Password,
                    Pooling = false,
                    Timeout = 30,
                    CommandTimeout = 300 // 5 minutes for large tables
                };

                await _dbSyncService.SyncAsync(builder.ConnectionString);
                TempData["success"] = "Database synchronized successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error during sync: {ex.Message}";
                return View("Index", model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
