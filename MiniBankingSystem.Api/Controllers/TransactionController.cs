using Microsoft.AspNetCore.Mvc;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Application.Interfaces;

namespace MiniBankingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(
        ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactionsByAccountId(
        Guid accountId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var transactions =
            await _transactionService.GetTransactionsByAccountIdAsync(
                accountId,
                pageNumber,
                pageSize);

        return Ok(transactions);
    }
}