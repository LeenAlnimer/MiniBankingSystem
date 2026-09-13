using Microsoft.AspNetCore.Mvc;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Application.Interfaces;

namespace MiniBankingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> CreateAccount(
        CreateAccountDto dto)
    {
        var account = await _accountService.CreateAccountAsync(dto);

        return Ok(account);
    }

    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAccounts()
    {
        var accounts = await _accountService.GetAccountsAsync();

        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetAccountById(Guid id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);

        if (account == null)
        {
            return NotFound();
        }

        return Ok(account);
    }
}