using OrderProcessingAPI.Models;

public class DashboardViewModel
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalProductsSold { get; set; }
    public List<Order> RecentOrders { get; set; }
    public List<MostSoldProductViewModel> MostSoldProducts { get; set; }  
}

public class MostSoldProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int QuantitySold { get; set; }
}
