using MiniBankingSystem.Application.DTOs;

namespace MiniBankingSystem.Application.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionDto>> GetTransactionsByAccountIdAsync(
        Guid accountId,
        int pageNumber,
        int pageSize);
}