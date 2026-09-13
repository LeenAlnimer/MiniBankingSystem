using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Domain.Entities;

namespace MiniBankingSystem.Application.Services;

public class AccountService : IAccountService
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public AccountService(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
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
        // Redis key
        var cacheKey = $"account:{id}";

        // 1. Try to get account from Redis
        var cachedAccount = await _cacheService.GetAsync(cacheKey);

        if (cachedAccount != null)
        {
            // Cache Hit
            return JsonSerializer.Deserialize<AccountDto>(
                cachedAccount);
        }

        // 2. Cache Miss → get account from PostgreSQL
        var account = await _context.Accounts
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

        if (account == null)
        {
            return null;
        }

        // 3. Serialize account to JSON
        var accountJson = JsonSerializer.Serialize(account);

        // 4. Save account in Redis for 5 minutes
        await _cacheService.SetAsync(
            cacheKey,
            accountJson,
            TimeSpan.FromMinutes(5));

        return account;
    }

    public async Task<AccountDto> DepositAsync(DepositDto dto)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == dto.AccountId);

        if (account == null)
        {
            throw new ArgumentException("Account not found.");
        }

        if (dto.Amount <= 0)
        {
            throw new ArgumentException(
                "Deposit amount must be greater than zero.");
        }

        account.Balance += dto.Amount;

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Type = "Deposit",
            Amount = dto.Amount,
            Currency = account.Currency,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        // Remove old cached account because balance changed
        await _cacheService.RemoveAsync(
            $"account:{account.Id}");

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

    public async Task<AccountDto> WithdrawAsync(WithdrawDto dto)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == dto.AccountId);

        if (account == null)
        {
            throw new ArgumentException("Account not found.");
        }

        if (dto.Amount <= 0)
        {
            throw new ArgumentException(
                "Withdrawal amount must be greater than zero.");
        }

        if (account.Balance < dto.Amount)
        {
            throw new InvalidOperationException(
                "Insufficient balance.");
        }

        account.Balance -= dto.Amount;

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Type = "Withdrawal",
            Amount = dto.Amount,
            Currency = account.Currency,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        // Remove old cached account because balance changed
        await _cacheService.RemoveAsync(
            $"account:{account.Id}");

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

    public async Task TransferAsync(TransferDto dto)
    {
        var fromAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == dto.FromAccountId);

        if (fromAccount == null)
        {
            throw new ArgumentException(
                "Source account not found.");
        }

        var toAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == dto.ToAccountId);

        if (toAccount == null)
        {
            throw new ArgumentException(
                "Destination account not found.");
        }

        if (dto.FromAccountId == dto.ToAccountId)
        {
            throw new ArgumentException(
                "Source and destination accounts must be different.");
        }

        if (dto.Amount <= 0)
        {
            throw new ArgumentException(
                "Transfer amount must be greater than zero.");
        }

        if (fromAccount.Balance < dto.Amount)
        {
            throw new InvalidOperationException(
                "Insufficient balance.");
        }

        if (fromAccount.Currency != toAccount.Currency)
        {
            throw new InvalidOperationException(
                "Accounts must use the same currency.");
        }

        fromAccount.Balance -= dto.Amount;

        toAccount.Balance += dto.Amount;

        var withdrawalTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = fromAccount.Id,
            Type = "Transfer Out",
            Amount = dto.Amount,
            Currency = fromAccount.Currency,
            CreatedAt = DateTime.UtcNow
        };

        var depositTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = toAccount.Id,
            Type = "Transfer In",
            Amount = dto.Amount,
            Currency = toAccount.Currency,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(withdrawalTransaction);
        _context.Transactions.Add(depositTransaction);

        await _context.SaveChangesAsync();

        // Remove old cached data for both accounts
        await _cacheService.RemoveAsync(
            $"account:{fromAccount.Id}");

        await _cacheService.RemoveAsync(
            $"account:{toAccount.Id}");
    }

    private string GenerateAccountNumber()
    {
        return $"JO-{Random.Shared.Next(100000, 999999)}";
    }
}