namespace OrderProcessingAPI.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Amount { get; set; }
        public Order Order { get; set; }
    }
}
