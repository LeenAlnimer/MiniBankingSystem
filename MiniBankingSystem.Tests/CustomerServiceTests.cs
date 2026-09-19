using Microsoft.EntityFrameworkCore;
using Moq;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Application.Services;
using MiniBankingSystem.Domain.Entities;

namespace MiniBankingSystem.Tests;

public class CustomerServiceTests
{
    [Fact]
    public async Task CreateCustomerAsync_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Customers)
            .Returns(db.Customers);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var service = new CustomerService(
            contextMock.Object);

        var dto = new CreateCustomerDto
        {
            FullName = "Leen Alnimer",
            Email = "leen@test.com",
            PhoneNumber = "0790000000"
        };

        // Act
        var result = await service.CreateCustomerAsync(dto);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Leen Alnimer", result.FullName);
        Assert.Equal("leen@test.com", result.Email);
        Assert.Equal("0790000000", result.PhoneNumber);
        Assert.NotEqual(default, result.CreatedAt);

        var customer = await db.Customers
            .FirstOrDefaultAsync();

        Assert.NotNull(customer);
        Assert.Equal(result.Id, customer.Id);
        Assert.Equal("Leen Alnimer", customer.FullName);
        Assert.Equal("leen@test.com", customer.Email);
        Assert.Equal("0790000000", customer.PhoneNumber);

        contextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task GetCustomersAsync_ShouldReturnAllCustomers()
    {
        // Arrange
        var customer1 = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Ahmad Ali",
            Email = "ahmad@test.com",
            PhoneNumber = "0791111111",
            CreatedAt = DateTime.UtcNow
        };

        var customer2 = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Sara Ahmad",
            Email = "sara@test.com",
            PhoneNumber = "0792222222",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Customers.AddRange(customer1, customer2);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Customers)
            .Returns(db.Customers);

        var service = new CustomerService(
            contextMock.Object);

        // Act
        var result = await service.GetCustomersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstCustomer = result
            .First(x => x.Id == customer1.Id);

        var secondCustomer = result
            .First(x => x.Id == customer2.Id);

        Assert.Equal("Ahmad Ali", firstCustomer.FullName);
        Assert.Equal("ahmad@test.com", firstCustomer.Email);
        Assert.Equal("0791111111", firstCustomer.PhoneNumber);

        Assert.Equal("Sara Ahmad", secondCustomer.FullName);
        Assert.Equal("sara@test.com", secondCustomer.Email);
        Assert.Equal("0792222222", secondCustomer.PhoneNumber);
    }


    [Fact]
    public async Task GetCustomerByIdAsync_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            FullName = "Omar Khaled",
            Email = "omar@test.com",
            PhoneNumber = "0793333333",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Customers)
            .Returns(db.Customers);

        var service = new CustomerService(
            contextMock.Object);

        // Act
        var result = await service.GetCustomerByIdAsync(customerId);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(customerId, result.Id);
        Assert.Equal("Omar Khaled", result.FullName);
        Assert.Equal("omar@test.com", result.Email);
        Assert.Equal("0793333333", result.PhoneNumber);
    }


    [Fact]
    public async Task GetCustomerByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Customers)
            .Returns(db.Customers);

        var service = new CustomerService(
            contextMock.Object);

        // Act
        var result = await service.GetCustomerByIdAsync(customerId);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task UpdateCustomerAsync_ShouldUpdateCustomerSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            FullName = "Old Name",
            Email = "old@test.com",
            PhoneNumber = "0794444444",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Customers)
            .Returns(db.Customers);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var service = new CustomerService(
            contextMock.Object);

        var dto = new CreateCustomerDto
        {
            FullName = "New Name",
            Email = "new@test.com",
            PhoneNumber = "0795555555"
        };

        // Act
        var result = await service.UpdateCustomerAsync(
            customerId,
            dto);

        // Assert
        Assert.True(result);

        var updatedCustomer = await db.Customers
            .FirstOrDefaultAsync(x => x.Id == customerId);

        Assert.NotNull(updatedCustomer);

        Assert.Equal("New Name", updatedCustomer.FullName);
        Assert.Equal("new@test.com", updatedCustomer.Email);
        Assert.Equal("0795555555", updatedCustomer.PhoneNumber);

        contextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task UpdateCustomerAsync_ShouldReturnFalse_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Customers)
            .Returns(db.Customers);

        var service = new CustomerService(
            contextMock.Object);

        var dto = new CreateCustomerDto
        {
            FullName = "New Name",
            Email = "new@test.com",
            PhoneNumber = "0795555555"
        };

        // Act
        var result = await service.UpdateCustomerAsync(
            customerId,
            dto);

        // Assert
        Assert.False(result);

        contextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Fact]
    public async Task DeleteCustomerAsync_ShouldDeleteCustomerSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            FullName = "Delete Me",
            Email = "delete@test.com",
            PhoneNumber = "0796666666",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Customers)
            .Returns(db.Customers);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var service = new CustomerService(
            contextMock.Object);

        // Act
        var result = await service.DeleteCustomerAsync(customerId);

        // Assert
        Assert.True(result);

        var deletedCustomer = await db.Customers
            .FirstOrDefaultAsync(x => x.Id == customerId);

        Assert.Null(deletedCustomer);

        contextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task DeleteCustomerAsync_ShouldReturnFalse_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Customers)
            .Returns(db.Customers);

        var service = new CustomerService(
            contextMock.Object);

        // Act
        var result = await service.DeleteCustomerAsync(customerId);

        // Assert
        Assert.False(result);

        contextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}