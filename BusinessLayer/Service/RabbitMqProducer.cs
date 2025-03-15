using System;
using RabbitMQ.Client;
using BusinessLayer.Interface;
using ModelLayer.DTO;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
namespace BusinessLayer.Service
{
    /// <summary>
    /// This class produces and publish the message
    /// </summary>
    public class RabbitMqProducer : IRabbitMqProducer
    {
        private readonly IConfiguration _configuration;
        //Constructor 
        public RabbitMqProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        //method to publish the message to rabbitmq
        public void PublishMessage(UserEventDTO userEvent)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"],
            };
            //creating the connection
            using var connection = factory.CreateConnection();
            //creating the channel 
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: _configuration["RabbitMQ:Exchange"], type: ExchangeType.Direct);
            channel.QueueDeclare(queue: _configuration["RabbitMQ:Queue"], durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queue: _configuration["RabbitMQ:Queue"], exchange: _configuration["RabbitMQ:Exchange"], routingKey: _configuration["RabbitMQ:RoutingKey"]);
            //message to be sent 
            var messageBody = JsonSerializer.Serialize(userEvent);
            var body = Encoding.UTF8.GetBytes(messageBody);
            //publish the message 
            channel.BasicPublish(exchange: _configuration["RabbitMQ:Exchange"],
                routingKey: _configuration["RabbitMQ:RoutingKey"],
                body: body,
                basicProperties: null);

        }

    }
}
