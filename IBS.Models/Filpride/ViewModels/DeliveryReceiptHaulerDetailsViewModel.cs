using System.ComponentModel.DataAnnotations;

namespace IBS.Models.Filpride.ViewModels
{
    public class DeliveryReceiptHaulerDetailsViewModel : DeliveryReceiptViewModel
    {
        [StringLength(200)]
        public string? HaulerInvoiceNo { get; set; }

        public DateOnly? HaulerInvoiceDate { get; set; }

        public decimal HaulerInvoiceAmount { get; set; }
    }
}
