using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OrderProcessingAPI.Data;
using OrderProcessingAPI.Models;

namespace OrderProcessingAPI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalOrders = _context.Orders.Count(),
                TotalRevenue = _context.Orders.Sum(o => o.TotalAmount),
                TotalProductsSold = _context.OrderDetails.Sum(od => od.Amount),
                RecentOrders = _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList(),
                MostSoldProducts = _context.OrderDetails
                    .GroupBy(od => od.ProductId)
                    .Select(g => new MostSoldProductViewModel
                    {
                        ProductId = g.Key,
                        ProductName = _context.Products
                                       .Where(p => p.Id == g.Key)
                                       .Select(p => p.Description)
                                       .FirstOrDefault(), 
                        QuantitySold = g.Sum(od => od.Amount)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }
    }
}
