using IBS.DataAccess.Repository.IRepository;
using IBS.DTOs;
using IBS.Models.Filpride.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ICreditMemoRepository: IRepository<FilprideCreditMemo>
    {
        Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default);
        Task PostAsync(FilprideCreditMemo model, CancellationToken cancellationToken = default,
            List<AccountTitleDto>? accountTitlesDto = null, bool saveChanges = true);
    }
}
