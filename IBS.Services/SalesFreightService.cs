using IBS.DataAccess.Data;
using IBS.Models.Filpride.Integrated;
using Microsoft.EntityFrameworkCore;

namespace IBS.Services
{
    public class SalesFreightService
    {
        private readonly ApplicationDbContext _db;

        public SalesFreightService(ApplicationDbContext db)
        {
            _db = db;
        }

        public static string? ValidateRate(decimal freight)
        {
            return freight < 0m || freight > 99999999999999.9999m || decimal.Round(freight, 4) != freight
                ? "Freight must be nonnegative and have at most four decimal places."
                : null;
        }

        public async Task<string?> ValidateAsync(int customerId, decimal freight, decimal? savedFreight,
            bool delivered, CancellationToken cancellationToken)
        {
            var rateError = ValidateRate(freight);
            if (rateError != null)
            {
                return rateError;
            }

            if (savedFreight == freight)
            {
                return null;
            }

            if (delivered)
            {
                return "Freight cannot be changed after delivery. Contact MIS for assistance.";
            }

            var standardFreight = await _db.FilprideCustomers
                .Where(c => c.CustomerId == customerId)
                .Select(c => (decimal?)c.StandardFreight)
                .SingleOrDefaultAsync(cancellationToken);

            if (standardFreight == null)
            {
                return "Customer not found.";
            }

            return freight > standardFreight
                ? $"Freight exceeds the customer standard of {standardFreight:N4}. Obtain signed AR approval, then contact MIS for assistance."
                : null;
        }

        public async Task<string?> ValidateCosAsync(int customerId, decimal freight,
            FilprideCustomerOrderSlip? existing, CancellationToken cancellationToken)
        {
            var delivered = false;
            if (existing != null && (existing.Freight != freight || existing.CustomerId != customerId))
            {
                delivered = existing.IsDelivered || await _db.FilprideDeliveryReceipts.AnyAsync(dr =>
                    dr.DeliveredDate != null && dr.CanceledBy == null && dr.VoidedBy == null
                    && (dr.CustomerOrderSlipId == existing.CustomerOrderSlipId
                        || dr.Details.Any(d => d.CustomerOrderSlipId == existing.CustomerOrderSlipId)), cancellationToken);
            }

            return await ValidateAsync(customerId, freight,
                existing?.CustomerId == customerId ? existing.Freight : null, delivered, cancellationToken);
        }
    }
}
