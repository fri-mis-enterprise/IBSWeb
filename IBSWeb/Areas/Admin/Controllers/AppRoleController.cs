using System.Linq.Dynamic.Core;
using IBS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IBSWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles = "Admin")]
    public class AppRoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AppRoleController> _logger;

        public AppRoleController(RoleManager<IdentityRole> roleManager, ILogger<AppRoleController> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            if (!await _roleManager.RoleExistsAsync(model.Name!))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Name!));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetRolesList([FromForm] DataTablesParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var queried = _roleManager.Roles;

                foreach (DataTablesColumn column in parameters.Columns)
                {
                    List<string>? values = column.ColumnControl?.List;

                    if (values == null || values.Count == 0)
                    {
                        continue;
                    }

                    values = values
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();

                    if (values.Count == 0)
                    {
                        continue;
                    }

                    string columnName = column.Data;

                    switch (columnName)
                    {
                        case "name":
                            queried = queried.Where(x =>
                                values.Contains(x.Name!));
                            break;
                    }
                }

                // Global search
                if (!string.IsNullOrEmpty(parameters.Search.Value))
                {
                    var searchValue = parameters.Search.Value.ToLower();

                    queried = queried
                    .Where(r =>
                        r.Name!.ToLower().Contains(searchValue) == true
                        );
                }

                // Sorting
                if (parameters.Order?.Count > 0)
                {
                    var orderColumn = parameters.Order[0];
                    var columnName = parameters.Columns[orderColumn.Column].Name;
                    var sortDirection = orderColumn.Dir.ToLower() == "asc" ? "ascending" : "descending";
                    queried = queried
                        .AsQueryable()
                        .OrderBy($"{columnName} {sortDirection}");
                }

                var totalRecords = queried.Count();
                var pagedData = queried
                    .Select(r  => new
                    {
                        r.Name,
                    })
                    .Skip(parameters.Start)
                    .Take(parameters.Length)
                    .ToList();

                var columnControlOptionList = queried
                    .Select(x => new
                    {
                        Name = x.Name
                    })
                    .ToList();

                return Json(new
                {
                    draw = parameters.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = pagedData,
                    columnControl = new Dictionary<string, object>
                    {
                        ["Name"] = columnControlOptionList.Select(x => x.Name).Where(value => value != null).Distinct()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get roles.");
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
