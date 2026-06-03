using IBS.DTOs;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Filpride;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ILockedPeriodAdjustmentRepository : IRepository<LockedPeriodAdjustment>
    {
        Task AddIfPeriodPostedAsync(LockedPeriodAdjustmentRequestDto request, CancellationToken cancellationToken = default);
    }
}
