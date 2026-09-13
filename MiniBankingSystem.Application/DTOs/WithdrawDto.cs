namespace MiniBankingSystem.Application.DTOs;

public class WithdrawDto
{
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }
}