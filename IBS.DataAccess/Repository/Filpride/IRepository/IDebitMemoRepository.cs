using IBS.DataAccess.Repository.IRepository;
using IBS.DTOs;
using IBS.Models.Filpride;
using IBS.Models.Filpride.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IDebitMemoRepository: IRepository<FilprideDebitMemo>
    {
        Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default);
        Task PostAsync(FilprideDebitMemo model, CancellationToken cancellationToken = default,
            List<AccountTitleDto>? accountTitlesDto = null, bool saveChanges = true);
    }
}
