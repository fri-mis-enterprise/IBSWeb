using IBS.DataAccess.Repository.IRepository;
using IBS.DTOs;
using IBS.Models.Filpride.AccountsReceivable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ISalesInvoiceRepository : IRepository<FilprideSalesInvoice>
    {
        Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default);

        Task<SalesInvoiceTaxBalanceDto?> GetTaxBalanceAsync(int salesInvoiceId,
            int? excludedCollectionReceiptId = null,
            CancellationToken cancellationToken = default);

        Task<List<SalesInvoiceCollectionDetailsDto>> GetCollectionDetailsAsync(int[] salesInvoiceIds,
            int? excludedCollectionReceiptId = null,
            CancellationToken cancellationToken = default);

        Task RecalculateTaxBalancesAsync(int salesInvoiceId, CancellationToken cancellationToken = default);

        Task RecalculateTaxBalancesAsync(int[] salesInvoiceIds, CancellationToken cancellationToken = default);
    }
}
