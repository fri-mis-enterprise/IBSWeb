using IBS.DataAccess.Repository.IRepository;
using IBS.DTOs;
using IBS.Models.Filpride.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IServiceInvoiceRepository : IRepository<FilprideServiceInvoice>
    {
        Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default);

        Task<ServiceInvoiceTaxBalanceDto?> GetTaxBalanceAsync(int serviceInvoiceId,
            int? excludedCollectionReceiptId = null,
            CancellationToken cancellationToken = default);

        Task RecalculateTaxBalancesAsync(int serviceInvoiceId, CancellationToken cancellationToken = default);

        Task PostAsync(FilprideServiceInvoice model, CancellationToken cancellationToken = default);
    }
}
