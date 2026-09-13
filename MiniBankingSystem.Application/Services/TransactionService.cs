using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Application.Interfaces;

namespace MiniBankingSystem.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IApplicationDbContext _context;

    public TransactionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransactionDto>> GetTransactionsByAccountIdAsync(
        Guid accountId,
        int pageNumber,
        int pageSize)
    {
        var accountExists = await _context.Accounts
            .AnyAsync(a => a.Id == accountId);

        if (!accountExists)
        {
            throw new ArgumentException("Account not found.");
        }

        return await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                Type = t.Type,
                Amount = t.Amount,
                Currency = t.Currency,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }
}