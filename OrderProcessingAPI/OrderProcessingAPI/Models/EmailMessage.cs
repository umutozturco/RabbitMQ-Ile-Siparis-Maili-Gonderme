public class EmailMessage
{
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public List<ProductDetail> Products { get; set; } = new List<ProductDetail>();  
    public decimal TotalAmount { get; set; }
}

public class ProductDetail  
{
    public string Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
