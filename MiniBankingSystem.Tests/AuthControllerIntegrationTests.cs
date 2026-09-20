using System.Net;
using System.Text.Json;
using MiniBankingSystem.Tests.Infrastructure;

namespace MiniBankingSystem.Tests;

public class AuthControllerIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenUserIsValid()
    {
        var username = $"leen_{Guid.NewGuid():N}";
        var email = $"{Guid.NewGuid():N}@test.com";

        var response = await _client.PostAsync(
            $"/api/Auth/register?username={username}&email={email}&password=Password123",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "User registered successfully.",
            json);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
    {
        var username = $"leen_{Guid.NewGuid():N}";
        var email = $"{Guid.NewGuid():N}@test.com";

        var firstResponse = await _client.PostAsync(
            $"/api/Auth/register?username={username}&email={email}&password=Password123",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        var secondResponse = await _client.PostAsync(
            $"/api/Auth/register?username={username}&email={Guid.NewGuid():N}@test.com&password=Password123",
            null);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        var username1 = $"leen_{Guid.NewGuid():N}";
        var username2 = $"leen_{Guid.NewGuid():N}";
        var email = $"{Guid.NewGuid():N}@test.com";

        var firstResponse = await _client.PostAsync(
            $"/api/Auth/register?username={username1}&email={email}&password=Password123",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        var secondResponse = await _client.PostAsync(
            $"/api/Auth/register?username={username2}&email={email}&password=Password123",
            null);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var username = $"leen_{Guid.NewGuid():N}";
        var email = $"{Guid.NewGuid():N}@test.com";
        var password = "Password123";

        var registerResponse = await _client.PostAsync(
            $"/api/Auth/register?username={username}&email={email}&password={password}",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var loginResponse = await _client.PostAsync(
            $"/api/Auth/login?username={username}&password={password}",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var json =
            await loginResponse.Content.ReadAsStringAsync();

        using var document =
            JsonDocument.Parse(json);

        var token =
            document.RootElement
                .GetProperty("token")
                .GetString();

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsInvalid()
    {
        var username = $"leen_{Guid.NewGuid():N}";
        var email = $"{Guid.NewGuid():N}@test.com";

        var registerResponse = await _client.PostAsync(
            $"/api/Auth/register?username={username}&email={email}&password=Password123",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var loginResponse = await _client.PostAsync(
            $"/api/Auth/login?username={username}&password=WrongPassword",
            null);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenUsernameDoesNotExist()
    {
        var loginResponse = await _client.PostAsync(
            $"/api/Auth/login?username=unknown_{Guid.NewGuid():N}&password=Password123",
            null);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            loginResponse.StatusCode);
    }
}