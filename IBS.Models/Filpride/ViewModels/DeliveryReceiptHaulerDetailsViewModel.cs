using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models.Filpride.ViewModels
{
    public class DeliveryReceiptHaulerDetailsViewModel
    {
        public int DeliveryReceiptId { get; set; }

        public DateOnly? Date { get; set; }

        public string? ManualDrNo { get; set; }

        #region--Customer

        public string? CustomerName { get; set; }

        public string? CustomerAddress { get; set; }

        public string? CustomerTin { get; set; }

        #endregion

        #region--COS

        public string? CustomerOrderSlipNo { get; set; }

        public string? Product { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal? CosVolume { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal? RemainingVolume { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal? Price { get; set; }

        public string? DeliveryOption { get; set; }

        #endregion

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = false)]
        public decimal? Volume { get; set; }

        public string? ATLNo { get; set; }

        public string? Driver { get; set; }

        public string? PlateNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal? Freight { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal? ECC { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal? TotalAmount { get; set; }

        public string? Remarks { get; set; }

        [StringLength(200)]
        public string? HaulerInvoiceNo { get; set; }

        public DateOnly? HaulerInvoiceDate { get; set; }

        public decimal HaulerInvoiceAmount { get; set; }

        public string? HaulerName { get; set; }
    }
}
