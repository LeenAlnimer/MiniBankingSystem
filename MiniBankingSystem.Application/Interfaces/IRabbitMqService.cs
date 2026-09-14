namespace MiniBankingSystem.Application.Interfaces;

public interface IRabbitMqService
{
    Task PublishAsync(string message);
}