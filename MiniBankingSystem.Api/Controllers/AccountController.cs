using Microsoft.AspNetCore.Mvc;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Application.Interfaces;

namespace MiniBankingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IExchangeRateService _exchangeRateService;

    public AccountController(
        IAccountService accountService,
        IRabbitMqService rabbitMqService,
        IExchangeRateService exchangeRateService)
    {
        _accountService = accountService;
        _rabbitMqService = rabbitMqService;
        _exchangeRateService = exchangeRateService;
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
    public async Task<ActionResult<AccountDto>> GetAccountById(
        Guid id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);

        if (account == null)
        {
            return NotFound();
        }

        return Ok(account);
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<AccountDto>> Deposit(
        DepositDto dto)
    {
        var account = await _accountService.DepositAsync(dto);

        return Ok(account);
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<AccountDto>> Withdraw(
        WithdrawDto dto)
    {
        var account = await _accountService.WithdrawAsync(dto);

        return Ok(account);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
        TransferDto dto)
    {
        await _accountService.TransferAsync(dto);

        return Ok(new
        {
            message = "Transfer completed successfully."
        });
    }
    [HttpGet("exchange-rate")]
    public async Task<IActionResult> GetExchangeRate(
    string fromCurrency,
    string toCurrency)
    {
        var rate = await _exchangeRateService.GetExchangeRateAsync(
            fromCurrency,
            toCurrency);

        return Ok(new
        {
            fromCurrency,
            toCurrency,
            rate
        });
    }

    [HttpPost("test-message")]
    public async Task<IActionResult> TestMessage()
    {
        await _rabbitMqService.PublishAsync(
            "Hello from MiniBankingSystem!");

        return Ok(new
        {
            message = "Message sent to RabbitMQ successfully."
        });
    }
}