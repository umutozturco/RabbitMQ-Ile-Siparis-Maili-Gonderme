using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrderProcessingAPI.Data;
using OrderProcessingAPI.Models;
using OrderProcessingAPI.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrderProcessingAPI.Controllers
{
    public class RabbitMQController : Controller
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly ApplicationDbContext _context;

        public RabbitMQController(RabbitMQService rabbitMQService, ApplicationDbContext context)
        {
            _rabbitMQService = rabbitMQService;
            _context = context;
        }

        [HttpGet("/RabbitMQ")]
        public IActionResult Index()
        {
            var products = _context.Products
                .Select(p => new { p.Id, p.Description, p.UnitPrice })  // Id, Description ve UnitPrice bilgilerini al
                .ToList();

            ViewBag.ProductDescriptions = new SelectList(products, "Id", "Description");

            return View();
        }

        [HttpPost("/RabbitMQ/SendEmail")]
        public async Task<IActionResult> SendEmail(string CustomerName, string CustomerEmail, string? CustomerGSM, List<int> ProductId, List<int> Quantities)
        {
            var products = _context.Products
                .Where(p => ProductId.Contains(p.Id))
                .Select(p => new { p.Id, p.Description, p.UnitPrice })
                .ToList();

            if (!products.Any())
            {
                ViewData["Message"] = "Invalid products selected.";
                ViewData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }

            decimal totalAmount = products.Sum(p => p.UnitPrice * Quantities[ProductId.IndexOf(p.Id)]);

            var order = new Order
            {
                CustomerName = CustomerName,
                CustomerEmail = CustomerEmail,
                CustomerGSM = CustomerGSM,
                TotalAmount = totalAmount,
                OrderDate = DateTime.Now,
                OrderDetails = products.Select(p => new OrderDetail
                {
                    ProductId = p.Id,
                    UnitPrice = p.UnitPrice,
                    Amount = Quantities[ProductId.IndexOf(p.Id)]
                }).ToList()
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var emailMessage = new EmailSendingService.EmailMessage
            {
                CustomerName = CustomerName,
                CustomerEmail = CustomerEmail,
                TotalAmount = totalAmount,
                Products = products.Select(p => new EmailSendingService.ProductDetail
                {
                    Description = p.Description,
                    UnitPrice = p.UnitPrice,
                    Quantity = Quantities[ProductId.IndexOf(p.Id)]
                }).ToList()
            };

            try
            {
                bool result = await _rabbitMQService.SendMessageAsync(emailMessage);
                if (result)
                {
                    ViewData["Message"] = "Mail gönderimi yapıldı!";
                    ViewData["MessageType"] = "success";
                }
                else
                {
                    ViewData["Message"] = "Mail gönderimi yapılamadı!";
                    ViewData["MessageType"] = "danger";
                }
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "Mail gönderimi yapılamadı: " + ex.Message;
                ViewData["MessageType"] = "danger";
            }

            var productDescriptions = _context.Products
                .Select(p => new { p.Id, p.Description })
                .ToList();

            ViewBag.ProductDescriptions = new SelectList(productDescriptions, "Id", "Description");

            return View("Index");
        }
    }
}
