using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Domain.Entities;

namespace MiniBankingSystem.Application.Services;

public class AccountService : IAccountService
{
    private readonly IApplicationDbContext _context;

    public AccountService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto dto)
    {
        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == dto.CustomerId);

        if (!customerExists)
        {
            throw new ArgumentException("Customer not found.");
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            AccountNumber = GenerateAccountNumber(),
            Balance = 0,
            Currency = dto.Currency,
            CreatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

        return new AccountDto
        {
            Id = account.Id,
            CustomerId = account.CustomerId,
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            Currency = account.Currency,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<List<AccountDto>> GetAccountsAsync()
    {
        return await _context.Accounts
            .Select(a => new AccountDto
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                Currency = a.Currency,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid id)
    {
        return await _context.Accounts
            .Where(a => a.Id == id)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                Currency = a.Currency,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    private string GenerateAccountNumber()
    {
        return $"JO-{Random.Shared.Next(100000, 999999)}";
    }
}