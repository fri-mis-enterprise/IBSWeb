using IBS.Models.Mobility;

namespace IBS.Models.Mobility.ViewModels
{
    public class LubeDeliveryVM
    {
        public MobilityLubePurchaseHeader Header { get; set; } = null!;

        public IEnumerable<MobilityLubePurchaseDetail> Details { get; set; } = null!;

    }
}
