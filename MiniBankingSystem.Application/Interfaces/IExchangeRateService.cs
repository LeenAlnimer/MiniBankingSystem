namespace MiniBankingSystem.Application.Interfaces;

public interface IExchangeRateService
{
    Task<decimal> GetExchangeRateAsync(
        string fromCurrency,
        string toCurrency);
}