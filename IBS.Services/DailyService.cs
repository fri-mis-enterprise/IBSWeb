using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models;
using IBS.Models.Enums;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IBS.Services
{
    public class DailyService
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly ILogger<DailyService> _logger;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        public DailyService(ApplicationDbContext dbContext, ILogger<DailyService> logger,
            UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var today = DateOnly.FromDateTime(DateTimeHelper.GetCurrentPhilippineTime());

                await CosExpiration(today, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        private async Task CosExpiration(DateOnly today, CancellationToken cancellationToken)
        {
            var cosList = await _dbContext.FilprideCustomerOrderSlips
                .Where(cos => cos.ExpirationDate <= today
                              && cos.Status != nameof(CosStatus.Completed)
                              && cos.Status != nameof(CosStatus.Expired)
                              && cos.Status != nameof(CosStatus.Closed)
                              && cos.Status != nameof(CosStatus.Disapproved))
                .ToListAsync(cancellationToken);

            if (cosList.Count == 0)
            {
                return;
            }

            foreach (var cos in cosList)
            {
                // Record the current status before updating
                var previousStatus = cos.Status;

                // Update the status to Expired
                cos.Status = nameof(CosStatus.Expired);

                // Append the previous status and timestamp to the remarks
                cos.Remarks = $"Previous status: [{previousStatus}] updated to Expired on {today}. {cos.Remarks}";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
