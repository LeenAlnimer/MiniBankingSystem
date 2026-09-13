namespace MiniBankingSystem.Application.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string AccountNumber { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public string Currency { get; set; } = "JOD";

    public DateTime CreatedAt { get; set; }
} // the  main object  of  this  class  to  put  what will return as  a  response