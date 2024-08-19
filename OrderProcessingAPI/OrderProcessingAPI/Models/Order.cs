namespace OrderProcessingAPI.Models
{

    public class Order
    {
        public int Id { get; set; } 
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string? CustomerGSM { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } 
    }
}
