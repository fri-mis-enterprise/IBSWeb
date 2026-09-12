using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Filpride.AccountsPayable;
using IBS.Models.Filpride.Integrated;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IPurchaseOrderRepository : IRepository<FilpridePurchaseOrder>
    {
        Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetPurchaseOrderListAsyncByCode(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetPurchaseOrderListAsyncById(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetPurchaseOrderListAsyncBySupplier(int supplierId, CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetPurchaseOrderListAsyncBySupplierAndProduct(int supplierId, int productId, CancellationToken cancellationToken = default);

        Task<string> GenerateCodeForSubPoAsync(string purchaseOrderNo, CancellationToken cancellationToken = default);

        Task UpdateActualCostOnSalesAndReceiptsAsync(FilpridePOActualPrice model, CancellationToken cancellationToken = default);

        Task<decimal> GetPurchaseOrderCost(int purchaseOrderId, CancellationToken cancellationToken = default);

    }
}
