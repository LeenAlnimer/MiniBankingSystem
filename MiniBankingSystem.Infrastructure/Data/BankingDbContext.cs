using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Domain.Entities;
namespace MiniBankingSystem.Infrastructure.Data
{
    public class BankingDbContext : DbContext, IApplicationDbContext

    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public BankingDbContext(DbContextOptions<BankingDbContext> options)
    : base(options)
        {
        }//  constructor  for  configuration 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer
            modelBuilder.Entity<Customer>()
                .HasKey(c => c.Id);

            // Customer 1 -> Many Accounts
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Accounts)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Account
            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);

            // Account 1 -> Many Transactions
            modelBuilder.Entity<Account>()
                .HasMany(a => a.Transactions)
                .WithOne(t => t.Account)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction
            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.Id);

            // AccountNumber
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            // Money precision
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);



        }// EF core  configuration
    }

}
