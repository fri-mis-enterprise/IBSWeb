using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models;
using IBS.Models.Filpride.Books;
using IBS.Services;
using IBSWeb.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IBSWeb.Areas.Filpride.Controllers
{
    [Area(nameof(Filpride))]
    [Authorize(Roles = "Admin")]
    public class MonthlyPeriodController : Controller
    {
        private readonly ILogger<MonthlyPeriodController> _logger;

        private readonly IMonthlyClosureService _monthlyClosureService;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _dbContext;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IHubContext<NotificationHub> _hubContext;

        public MonthlyPeriodController(
            ILogger<MonthlyPeriodController> logger,
            IMonthlyClosureService monthlyClosureService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _monthlyClosureService = monthlyClosureService;
            _userManager = userManager;
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        private string GetUserFullName()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                   ?? User.Identity?.Name!;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TriggerMonthlyClosure(DateOnly monthDate, CancellationToken cancellationToken)
        {

            try
            {
                await _monthlyClosureService.CloseAsync(monthDate, User.Identity!.Name!, cancellationToken);

                FilprideAuditTrail auditTrailBook = new(
                    GetUserFullName(),
                    $"Close the book for the month of {monthDate:MMM yyyy}",
                    "Monthly Period");

                await _dbContext.FilprideAuditTrails.AddAsync(auditTrailBook, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                await NotifyAdminAsync($"{GetUserFullName()} closed the books for {monthDate:MMM yyyy}.",
                    cancellationToken);

                TempData["success"] = $"Month of {monthDate:MMM yyyy} closed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to close period. Posted by: {Username}", GetUserFullName());
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TriggerMonthlyOpening(DateOnly monthDate, CancellationToken cancellationToken)
        {

            try
            {
                await _monthlyClosureService.OpenAsync(monthDate, User.Identity!.Name!, cancellationToken);

                FilprideAuditTrail auditTrailBook = new(
                    GetUserFullName(),
                    $"Open the book for the month of {monthDate:MMM yyyy}",
                    "Monthly Period");

                await _dbContext.FilprideAuditTrails.AddAsync(auditTrailBook, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                await NotifyAdminAsync($"{GetUserFullName()} opened the books for {monthDate:MMM yyyy}.",
                    cancellationToken);

                TempData["success"] = $"Month of {monthDate:MMM yyyy} opened successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to open period. Open by: {Username}", GetUserFullName());
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task NotifyAdminAsync(string message, CancellationToken cancellationToken)
        {
            try
            {
                var adminName = "azh";
                var recipient = await _userManager.FindByNameAsync(adminName);

                if (recipient == null || !recipient.IsActive)
                {
                    _logger.LogWarning("Posting activity notification recipient {AdminName} was not found or is inactive", adminName);
                    return;
                }

                await _unitOfWork.Notifications.AddNotificationAsync(recipient.Id, message);

                var connectionIds = await _dbContext.HubConnections
                    .Where(connection => connection.UserName == recipient.UserName)
                    .Select(connection => connection.ConnectionId)
                    .ToListAsync(cancellationToken);

                foreach (var connectionId in connectionIds)
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ReceivedNotification", "You have a new message.", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver posting activity notification to azh");
            }
        }
    }
}
