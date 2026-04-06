namespace IBS.DTOs
{
    public class SupplierDto
    {
        public int SupplierId { get; set; }

        public string SupplierCode { get; set; } = null!;

        public string SupplierName { get; set; } = null!;

        public string SupplierAddress { get; set; } = null!;

        public string SupplierTin { get; set; } = null!;
    }
}
