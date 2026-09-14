using MiniBankingSystem.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace MiniBankingSystem.Infrastructure.Messaging;

public class RabbitMqService : IRabbitMqService
{
    private readonly ConnectionFactory _factory;

    public RabbitMqService()
    {
        _factory = new ConnectionFactory
        {
            HostName = "minibanking-rabbitmq",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };
    }

    public async Task PublishAsync(string message)
    {
        await using var connection =
            await _factory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync();

        // Create Exchange
        await channel.ExchangeDeclareAsync(
            exchange: "transaction-exchange",
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        // Create Queue
        await channel.QueueDeclareAsync(
            queue: "transaction-created",
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind Queue to Exchange
        await channel.QueueBindAsync(
            queue: "transaction-created",
            exchange: "transaction-exchange",
            routingKey: "transaction.created");

        // Convert message to bytes
        var body = Encoding.UTF8.GetBytes(message);

        // Publish message
        await channel.BasicPublishAsync(
            exchange: "transaction-exchange",
            routingKey: "transaction.created",
            body: body);
    }
}