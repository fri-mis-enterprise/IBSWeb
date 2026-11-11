namespace IBS.Models.Filpride.ViewModels
{
    public class DeliveryReceiptHaulerDetailsViewModel : DeliveryReceiptViewModel
    {
        public string? HaulerInvoiceNo { get; set; }

        public DateOnly? HaulerInvoiceDate { get; set; }

        public decimal HaulerInvoiceAmount { get; set; }
    }
}
