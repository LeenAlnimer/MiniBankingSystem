using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Application.Interfaces;

namespace MiniBankingSystem.Infrastructure.BackgroundJobs;

public class TransactionSummaryJob
{
    private readonly IApplicationDbContext _context;

    public TransactionSummaryJob(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RunAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var transactions = await _context.Transactions
            .Where(t => t.CreatedAt >= today &&
                        t.CreatedAt < tomorrow)
            .ToListAsync();

        var totalTransactions = transactions.Count;

        var totalAmount = transactions.Sum(t => t.Amount);

        Console.WriteLine(
            $"Transaction Summary: {totalTransactions} transactions, Total Amount: {totalAmount} JOD");
    }
}