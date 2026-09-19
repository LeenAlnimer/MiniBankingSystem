using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Domain.Entities;

namespace MiniBankingSystem.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<Customer> Customers { get; set; }
}