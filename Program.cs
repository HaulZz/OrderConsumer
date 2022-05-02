using System;
using System.Text;
using OrderConsumer.Services;
using OrderPlacer.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace OrderConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "Venda",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var order = System.Text.Json.JsonSerializer.Deserialize<Order>(message);
                        Console.WriteLine($"Order:   Product Id {order.Id }, Order Quantity: { order.Quantity}");
                        Console.WriteLine($"Before:  Product Id {DataBaseService.Get(order.Id).Id }, Order Quantity: { DataBaseService.Get(order.Id).Quantity}");
                        DataBaseService.Discount(order);
                        Console.WriteLine($"After:   Product Id {DataBaseService.Get(order.Id).Id }, Order Quantity: { DataBaseService.Get(order.Id).Quantity}");
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch
                    {
                        channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                };
                channel.BasicConsume(queue: "Venda",
                                    autoAck: false,
                                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
