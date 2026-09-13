using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Domain.Entities;

namespace MiniBankingSystem.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }

    DbSet<Account> Accounts { get; }

    DbSet<Transaction> Transactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}