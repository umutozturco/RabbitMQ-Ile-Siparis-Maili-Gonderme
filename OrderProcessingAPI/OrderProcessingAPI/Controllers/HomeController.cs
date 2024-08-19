using Microsoft.AspNetCore.Mvc;
using OrderProcessingAPI.Services;
using System;

namespace OrderProcessingAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly RabbitMQService _rabbitMQService;

        public HomeController(RabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("/Home/SendEmail")]
        public async Task<IActionResult> SendEmail(string CustomerName, string CustomerEmail, decimal TotalAmount)
        {
            var emailMessage = new EmailSendingService.EmailMessage
            {
                CustomerName = CustomerName,
                CustomerEmail = CustomerEmail,
                TotalAmount = TotalAmount
            };

            bool result = await _rabbitMQService.SendMessageAsync(emailMessage);

            ViewData["Message"] = result ? "Email gönderildi!" : "Email gönderimi başarısız.";
            ViewData["MessageType"] = result ? "success" : "danger";

            return View("Index");
        }
    }
}
