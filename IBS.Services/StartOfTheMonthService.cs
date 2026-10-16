using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Enums;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IBS.Services
{
    public class StartOfTheMonthService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<StartOfTheMonthService> _logger;

        private readonly ApplicationDbContext _dbContext;

        public StartOfTheMonthService(IUnitOfWork unitOfWork,
            ILogger<StartOfTheMonthService> logger, ApplicationDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var today = DateOnly.FromDateTime(DateTimeHelper.GetCurrentPhilippineTime());
                var previousMonthDate = today.AddMonths(-1);

                await GetTheUnliftedDrs(previousMonthDate);
                await SendNotificationToManagementAccounting(previousMonthDate);
                await SendNotificationToCNC(previousMonthDate);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task GetTheUnliftedDrs(DateOnly previousMonthDate)
        {
            try
            {
                var hasUnliftedDrs = await _dbContext.FilprideDeliveryReceipts
                    .AnyAsync(x => x.Date.Month == previousMonthDate.Month
                                   && x.Date.Year == previousMonthDate.Year
                                   && !x.HasReceivingReport);

                if (hasUnliftedDrs)
                {
                    var users = await _dbContext.ApplicationUsers
                        .Where(u => u.Department == SD.Department_TradeAndSupply
                                    || u.Department == SD.Department_ManagementAccounting)
                        .Select(u => u.Id)
                        .ToListAsync();

                    var message = $"There are still unlifted reports for {previousMonthDate:MMM yyyy}. " +
                                  $"Please ensure the lifting dates for the remaining DRs are recorded to avoid issues during the month-end closing. " +
                                  $"CC: Management Accounting";

                    await _unitOfWork.Notifications.AddNotificationToMultipleUsersAsync(users, message);

                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting the unlifted DRs for {Date}", previousMonthDate);
                throw;
            }
        }

        private async Task SendNotificationToManagementAccounting(DateOnly previousMonth)
        {
            var users = await _dbContext.ApplicationUsers
                .Where(u => u.IsActive && u.Department == SD.Department_ManagementAccounting)
                .Select(u => u.Id)
                .ToListAsync();

            var message = $"Kindly generate the journal voucher list for {previousMonth:MMM yyyy}.";

            await _unitOfWork.Notifications.AddNotificationToMultipleUsersAsync(users, message);
        }

        private async Task SendNotificationToCNC(DateOnly previousMonth)
        {
            var users = await _dbContext.ApplicationUsers
                .Where(u => u.IsActive && u.Department == SD.Department_CreditAndCollection)
                .Select(u => u.Id)
                .ToListAsync();

            var message = $"Please ensure the transaction fee is created before the system closes the books for {previousMonth:MMM yyyy}.";

            await _unitOfWork.Notifications.AddNotificationToMultipleUsersAsync(users, message);
        }
    }
}
