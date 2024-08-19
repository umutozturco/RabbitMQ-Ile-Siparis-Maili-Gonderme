using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderProcessingAPI.Services
{
    public class RabbitMQService : IDisposable
    {
        private readonly string _hostName;
        private readonly string _queueName;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQService(IOptions<RabbitMQSettings> rabbitMQSettings)
        {
            _hostName = rabbitMQSettings.Value.Host;
            _queueName = rabbitMQSettings.Value.QueueName;
            CreateConnection();
        }

        private void CreateConnection()
        {
            try
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    var factory = new ConnectionFactory { HostName = _hostName };
                    _connection = factory.CreateConnection();
                }

                if (_channel == null || !_channel.IsOpen)
                {
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create connection: {ex.Message}");
                
            }
        }

        public async Task<bool> SendMessageAsync<T>(T message)
        {
            try
            {
                if (ConnectionExists())
                {
                    var json = JsonSerializer.Serialize(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true; 

                    _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: properties, body: body);
                    Console.WriteLine($"Message sent to queue {_queueName}");

                    return await Task.FromResult(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending message: {ex.Message}");
            }

            return await Task.FromResult(false);
        }

        // Bağlantı Durumu Kontrolü
        private bool ConnectionExists()
        {
            if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
            {
                return true;
            }
            CreateConnection();
            return _connection != null && _connection.IsOpen;
        }

        // Kaynakları Serbest Bırakma
        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

    public class RabbitMQSettings
    {
        public string Host { get; set; }
        public string QueueName { get; set; }
    }
}
