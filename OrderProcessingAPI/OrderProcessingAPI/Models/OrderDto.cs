namespace OrderProcessingAPI.Models
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
   
    }
}   