namespace OrderProcessingAPI.Models
{
	public class Product
	{
        public int Id { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FormattedUnitPrice => UnitPrice.ToString("F2");
     
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime UpdateDate { get; set; } = DateTime.Now;

    }
}
