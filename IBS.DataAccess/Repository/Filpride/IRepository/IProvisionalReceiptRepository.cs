using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Filpride.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IProvisionalReceiptRepository : IRepository<FilprideProvisionalReceipt>
    {
        Task<string> GenerateSeriesNumberAsync(string company, string type, CancellationToken cancellationToken = default);
        Task DepositAsync(FilprideProvisionalReceipt provisionalReceipt, CancellationToken cancellationToken = default);
        Task ReturnedCheck(string prNo, string company, string userName, CancellationToken cancellationToken = default);
    }
}
