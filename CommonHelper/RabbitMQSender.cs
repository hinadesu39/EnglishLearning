using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommonHelper
{
    public class RabbitMQSender
    {
        private readonly IOptions<IntegrationEventRabbitMQOptions> option;

        public RabbitMQSender(IOptions<IntegrationEventRabbitMQOptions> option)
        {
            this.option = option;
        }
        public void Publish(string eventName, object? eventData)
        {
            var factory = new ConnectionFactory();
            factory.HostName = option.Value.HostName;//RabbitMQ服务器地址
            factory.DispatchConsumersAsync = true;
            string exchangeName = option.Value.ExchangeName;//交换机的名字
            using var conn = factory.CreateConnection();
            using (var channel = conn.CreateModel())//创建信道
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2;
                channel.ExchangeDeclare(exchange: exchangeName, type: "direct");//声明交换机
                byte[] body;
                if (eventData == null)
                {
                    body = new byte[0];
                }
                else
                {
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    body = JsonSerializer.SerializeToUtf8Bytes(eventData, eventData.GetType(), options);
                }
                channel.BasicPublish(
                exchange: exchangeName,
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: body);
            }
        }
    }
}
