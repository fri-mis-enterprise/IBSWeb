using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Filpride.AccountsPayable;
using IBS.Models.Filpride.Books;
using IBS.Models.Filpride.Integrated;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IInventoryRepository : IRepository<FilprideInventory>
    {
        Task AddPurchaseToInventoryAsync(FilprideReceivingReport receivingReport, CancellationToken cancellationToken = default);

        Task AddSalesToInventoryAsync(FilprideDeliveryReceipt deliveryReceipt, CancellationToken cancellationToken = default);

        Task VoidInventory(FilprideInventory model, CancellationToken cancellationToken = default);

        Task ReCalculateInventoryAsync(List<FilprideInventory> inventories, CancellationToken cancellationToken = default);
    }
}
