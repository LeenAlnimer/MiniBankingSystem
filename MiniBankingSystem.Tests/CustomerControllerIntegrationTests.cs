using System.Net;
using System.Net.Http.Json;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Tests.Infrastructure;

namespace MiniBankingSystem.Tests;

public class CustomerControllerIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomerControllerIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnCreatedCustomer()
    {
        var request = new CreateCustomerDto
        {
            FullName = "Leen Alnimer",
            Email = "leen.create@test.com",
            PhoneNumber = "0791234567"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Customer",
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var customer =
            await response.Content
                .ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(customer);
        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Leen Alnimer", customer.FullName);
        Assert.Equal("leen.create@test.com", customer.Email);
        Assert.Equal("0791234567", customer.PhoneNumber);
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnCustomers()
    {
        var request = new CreateCustomerDto
        {
            FullName = "Get Customer",
            Email = "leen.get@test.com",
            PhoneNumber = "0791111111"
        };

        await _client.PostAsJsonAsync(
            "/api/Customer",
            request);

        var response = await _client.GetAsync(
            "/api/Customer");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var customers =
            await response.Content
                .ReadFromJsonAsync<List<CustomerDto>>();

        Assert.NotNull(customers);
        Assert.Contains(
            customers,
            customer =>
                customer.Email == "leen.get@test.com");
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnCustomer_WhenCustomerExists()
    {
        var request = new CreateCustomerDto
        {
            FullName = "Get By Id",
            Email = "leen.id@test.com",
            PhoneNumber = "0792222222"
        };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/Customer",
                request);

        var createdCustomer =
            await createResponse.Content
                .ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(createdCustomer);

        var response = await _client.GetAsync(
            $"/api/Customer/{createdCustomer.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var customer =
            await response.Content
                .ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(customer);
        Assert.Equal(
            createdCustomer.Id,
            customer.Id);

        Assert.Equal(
            "Get By Id",
            customer.FullName);

        Assert.Equal(
            "leen.id@test.com",
            customer.Email);
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();

        var response = await _client.GetAsync(
            $"/api/Customer/{customerId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnNoContent_WhenCustomerExists()
    {
        var createRequest = new CreateCustomerDto
        {
            FullName = "Before Update",
            Email = "leen.update@test.com",
            PhoneNumber = "0793333333"
        };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/Customer",
                createRequest);

        var createdCustomer =
            await createResponse.Content
                .ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(createdCustomer);

        var updateRequest = new CreateCustomerDto
        {
            FullName = "After Update",
            Email = "leen.updated@test.com",
            PhoneNumber = "0794444444"
        };

        var updateResponse =
            await _client.PutAsJsonAsync(
                $"/api/Customer/{createdCustomer.Id}",
                updateRequest);

        Assert.Equal(
            HttpStatusCode.NoContent,
            updateResponse.StatusCode);

        var getResponse =
            await _client.GetAsync(
                $"/api/Customer/{createdCustomer.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var updatedCustomer =
            await getResponse.Content
                .ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(updatedCustomer);
        Assert.Equal(
            "After Update",
            updatedCustomer.FullName);

        Assert.Equal(
            "leen.updated@test.com",
            updatedCustomer.Email);

        Assert.Equal(
            "0794444444",
            updatedCustomer.PhoneNumber);
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();

        var request = new CreateCustomerDto
        {
            FullName = "Unknown",
            Email = "unknown@test.com",
            PhoneNumber = "0795555555"
        };

        var response =
            await _client.PutAsJsonAsync(
                $"/api/Customer/{customerId}",
                request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_ShouldReturnNoContent_WhenCustomerExists()
    {
        var request = new CreateCustomerDto
        {
            FullName = "Delete Customer",
            Email = "leen.delete@test.com",
            PhoneNumber = "0796666666"
        };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/Customer",
                request);

        var createdCustomer =
            await createResponse.Content
                .ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(createdCustomer);

        var deleteResponse =
            await _client.DeleteAsync(
                $"/api/Customer/{createdCustomer.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var getResponse =
            await _client.GetAsync(
                $"/api/Customer/{createdCustomer.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();

        var response =
            await _client.DeleteAsync(
                $"/api/Customer/{customerId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}