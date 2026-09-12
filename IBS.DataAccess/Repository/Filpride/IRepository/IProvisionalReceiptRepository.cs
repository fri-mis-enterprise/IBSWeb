using IBS.DataAccess.Repository.IRepository;
using IBS.DTOs;
using IBS.Models.Filpride.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IProvisionalReceiptRepository : IRepository<FilprideProvisionalReceipt>
    {
        Task<string> GenerateSeriesNumberAsync(string company, string type, CancellationToken cancellationToken = default);
        Task PostAsync(int receiptId, string postedBy, SubAccountInfoDto? subAccount, CancellationToken cancellationToken = default);
        Task UnpostAsync(int receiptId, string unpostedBy, CancellationToken cancellationToken = default);
        Task ApplyClearingDateAsync(FilprideProvisionalReceipt provisionalReceipt, CancellationToken cancellationToken = default);
    }
}
