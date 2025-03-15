using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using System.IO;
using ModelLayer.DTO;


namespace RabbitMQ.Consumer
{
    public class UserConsumer
    {
        private readonly IConfiguration _configuration;

        public UserConsumer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConsumeMessage()
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _configuration["RabbitMQ:Queue"], durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var userEvent = JsonSerializer.Deserialize<UserEventDTO>(message);

                Console.WriteLine($"[User Event Received] Name: {userEvent.FirstName} {userEvent.LastName}, Email: {userEvent.Email}");

                // Simulate processing time
                System.Threading.Thread.Sleep(1000);
            };


            channel.BasicConsume(queue: _configuration["RabbitMQ:Queue"], autoAck: true, consumer: consumer);

            Console.WriteLine("Listening for messages...");
            Console.ReadLine();
        }
        static void Main()
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
            Console.WriteLine("RabbitMQ Host: " + configuration["RabbitMQ:Host"]);

            UserConsumer consumer = new UserConsumer(configuration);
            consumer.ConsumeMessage();

        }
    }
}
