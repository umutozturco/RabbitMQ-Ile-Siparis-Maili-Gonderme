namespace OrderProcessingAPI.Models
{
    public class OrderDetailDto
    {
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Amount { get; set; }
    }
}
