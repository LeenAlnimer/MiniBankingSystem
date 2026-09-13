namespace MiniBankingSystem.Application.DTOs;

public class DepositDto
{// deposit how much of  money  he  wants  to put in his  account  
    public Guid AccountId { get; set; }
    // to know  for  which account  we  should add amount
    public decimal Amount { get; set; }
}