using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MiniBankingSystem.Infrastructure.Messaging;

public class RabbitMqConsumer : BackgroundService
{
    private readonly ConnectionFactory _factory;

    public RabbitMqConsumer()
    {
        _factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connection =
            await _factory.CreateConnectionAsync();

        var channel =
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

        var consumer =
            new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine(
                $"RabbitMQ Message Received: {message}");

            // Acknowledge successful processing
            await channel.BasicAckAsync(
                eventArgs.DeliveryTag,
                multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "transaction-created",
            autoAck: false,
            consumer: consumer);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}