namespace IBS.DTOs
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }

        public string CustomerCode { get; set; } = null!;

        public string CustomerName { get; set; } = null!;

        public string CustomerAddress { get; set; } = null!;

        public string CustomerTin { get; set; } = null!;
        public string CustomerTerms { get; set; }= null!;
    }
}
