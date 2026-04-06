namespace IBS.Models.Mobility.ViewModels
{
    public class UnservedPurchaseOrderViewModel
    {
        public string StationName { get; set; } = null!;

        public List<MobilityPurchaseOrder> PurchaseOrders { get; set; } = null!;
    }
}
