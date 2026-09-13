namespace MiniBankingSystem.Application.DTOs;

public class CreateAccountDto
{
    public Guid CustomerId { get; set; }

    public string Currency { get; set; } = "JOD";
}