using System.Net.Http.Json;
using MiniBankingSystem.Application.Interfaces;

namespace MiniBankingSystem.Infrastructure.ExternalServices;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;

    public ExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    public async Task<decimal> GetExchangeRateAsync(
        string fromCurrency,
        string toCurrency)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"https://open.er-api.com/v6/latest/{fromCurrency}");

            response.EnsureSuccessStatusCode();

            var data = await response.Content
                .ReadFromJsonAsync<ExchangeRateResponse>();

            if (data == null ||
                !data.Rates.TryGetValue(toCurrency, out var rate))
            {
                throw new InvalidOperationException(
                    "Exchange rate not found.");
            }

            return rate;
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "External exchange rate service is unavailable.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException(
                "External exchange rate service request timed out.");
        }
    }
}

public class ExchangeRateResponse
{
    public Dictionary<string, decimal> Rates { get; set; } = new();
}