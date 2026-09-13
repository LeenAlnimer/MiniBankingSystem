using MiniBankingSystem.Application.DTOs;

namespace MiniBankingSystem.Application.Interfaces;

public interface IAccountService
{
    Task<AccountDto> CreateAccountAsync(CreateAccountDto dto);

    Task<List<AccountDto>> GetAccountsAsync();

    Task<AccountDto?> GetAccountByIdAsync(Guid id);

    Task<AccountDto> DepositAsync(DepositDto dto);

    Task<AccountDto> WithdrawAsync(WithdrawDto dto);

    Task TransferAsync(TransferDto dto);
}