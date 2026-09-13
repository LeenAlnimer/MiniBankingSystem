namespace MiniBankingSystem.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "JOD";

    public DateTime CreatedAt { get; set; }
}