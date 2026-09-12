namespace MiniBankingSystem.Domain.Entities;

public class Customer
{
    public Guid Id { get; set;  }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; }= string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreateAt { get; set; }
    public ICollection<Account> Accounts { get; set; } = new List<Account>();


}