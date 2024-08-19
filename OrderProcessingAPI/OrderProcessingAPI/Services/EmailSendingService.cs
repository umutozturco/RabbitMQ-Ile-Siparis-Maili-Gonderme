using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrderProcessingAPI.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OrderProcessingAPI.Services
{
    public class EmailSendingService : BackgroundService
    {
        private readonly RabbitMQSettings _rabbitMQSettings;
        private IConnection _connection;
        private IModel _channel;

        public EmailSendingService(IOptions<RabbitMQSettings> rabbitMQSettings)
        {
            _rabbitMQSettings = rabbitMQSettings.Value;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory { HostName = _rabbitMQSettings.Host };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

     
            _channel.QueueDeclare(queue: _rabbitMQSettings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var emailMessage = JsonSerializer.Deserialize<EmailMessage>(content);
                var product = JsonSerializer.Deserialize<Product>(content);


                bool isSent = await SendEmailAsync(emailMessage);

                if (isSent)
                {
                    Console.WriteLine($"Mail gönderildi: {emailMessage.CustomerEmail}");
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    Console.WriteLine($"Mail gönderilemedi: {emailMessage.CustomerEmail}");
                }
            };

            _channel.BasicConsume(_rabbitMQSettings.QueueName, false, consumer);

            return Task.CompletedTask;
        }

        private async Task<bool> SendEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("mailAdresiniz", "uygulamaSifreniz"),
                    EnableSsl = true,
                };

                var bodyBuilder = new StringBuilder();
                bodyBuilder.AppendLine($"Sayın {emailMessage.CustomerName},\n");
                bodyBuilder.AppendLine("Siparişiniz onaylanmıştır. İşte sipariş detaylarınız:\n");

                foreach (var product in emailMessage.Products)
                {
                    decimal totalPrice = product.UnitPrice * product.Quantity;
                    string formattedTotalPrice = totalPrice.ToString("F2");
                    bodyBuilder.AppendLine($"- {product.Description}: {product.Quantity} x {product.UnitPrice:F2} = {formattedTotalPrice}");
                }

                bodyBuilder.AppendLine($"\nToplam Tutar: {emailMessage.TotalAmount:F2}\n");
                bodyBuilder.AppendLine("Siparişiniz için teşekkür ederiz!");


                using var mailMessage = new MailMessage
                {
                    From = new MailAddress("mailAdresiniz"),
                    Subject = "Order Confirmation",
                    Body = bodyBuilder.ToString(),
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(emailMessage.CustomerEmail);
                await smtpClient.SendMailAsync(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email gönderim hatası: {ex.Message}");
                return false;
            }
        }


        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        public class EmailMessage
        {
            public string CustomerName { get; set; }
            public string CustomerEmail { get; set; }
            public decimal TotalAmount { get; set; }
            public List<ProductDetail> Products { get; set; } = new List<ProductDetail>();  
        }

        public class ProductDetail  
        {
            public string Description { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
        }
    }
}