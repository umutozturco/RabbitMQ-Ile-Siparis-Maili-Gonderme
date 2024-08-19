namespace OrderProcessingAPI.Models
{
	public class ProductDto
	{
        public int Id { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }

    }
}
