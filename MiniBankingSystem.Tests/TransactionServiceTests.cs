using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Application.Services;
using MiniBankingSystem.Domain.Entities;
using Moq;

namespace MiniBankingSystem.Tests;

public class TransactionServiceTests
{
    private static Mock<IApplicationDbContext> CreateContextMock(
        TestDbContext db)
    {
        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        return contextMock;
    }

    [Fact]
    public async Task GetTransactionsByAccountIdAsync_ShouldReturnTransactions_WhenAccountExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-123456",
            Balance = 500,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        db.Accounts.Add(account);

        db.Transactions.AddRange(
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Type = "Deposit",
                Amount = 200,
                Currency = "JOD",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Type = "Withdrawal",
                Amount = 50,
                Currency = "JOD",
                CreatedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        var contextMock = CreateContextMock(db);

        var service = new TransactionService(
            contextMock.Object);

        // Act
        var result = await service.GetTransactionsByAccountIdAsync(
            accountId,
            1,
            10);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Withdrawal", result[0].Type);
        Assert.Equal("Deposit", result[1].Type);
    }

    [Fact]
    public async Task GetTransactionsByAccountIdAsync_ShouldThrowException_WhenAccountDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var accountId = Guid.NewGuid();

        var contextMock = CreateContextMock(db);

        var service = new TransactionService(
            contextMock.Object);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetTransactionsByAccountIdAsync(
                    accountId,
                    1,
                    10));

        Assert.Equal("Account not found.", exception.Message);
    }

    [Fact]
    public async Task GetTransactionsByAccountIdAsync_ShouldReturnEmptyList_WhenAccountHasNoTransactions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var accountId = Guid.NewGuid();

        db.Accounts.Add(new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-123456",
            Balance = 500,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var contextMock = CreateContextMock(db);

        var service = new TransactionService(
            contextMock.Object);

        // Act
        var result = await service.GetTransactionsByAccountIdAsync(
            accountId,
            1,
            10);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTransactionsByAccountIdAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var accountId = Guid.NewGuid();

        db.Accounts.Add(new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-123456",
            Balance = 1000,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        });

        var baseTime = DateTime.UtcNow;

        db.Transactions.AddRange(
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Type = "Transaction 1",
                Amount = 100,
                Currency = "JOD",
                CreatedAt = baseTime.AddMinutes(-1)
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Type = "Transaction 2",
                Amount = 200,
                Currency = "JOD",
                CreatedAt = baseTime.AddMinutes(-2)
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Type = "Transaction 3",
                Amount = 300,
                Currency = "JOD",
                CreatedAt = baseTime.AddMinutes(-3)
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Type = "Transaction 4",
                Amount = 400,
                Currency = "JOD",
                CreatedAt = baseTime.AddMinutes(-4)
            });

        await db.SaveChangesAsync();

        var contextMock = CreateContextMock(db);

        var service = new TransactionService(
            contextMock.Object);

        // Act
        var result = await service.GetTransactionsByAccountIdAsync(
            accountId,
            2,
            2);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("Transaction 3", result[0].Type);
        Assert.Equal("Transaction 4", result[1].Type);
    }

    [Fact]
    public async Task GetTransactionsByAccountIdAsync_ShouldReturnOnlyTransactionsForRequestedAccount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var accountId = Guid.NewGuid();
        var anotherAccountId = Guid.NewGuid();

        db.Accounts.AddRange(
            new Account
            {
                Id = accountId,
                CustomerId = Guid.NewGuid(),
                AccountNumber = "JO-111111",
                Balance = 500,
                Currency = "JOD",
                CreatedAt = DateTime.UtcNow
            },
            new Account
            {
                Id = anotherAccountId,
                CustomerId = Guid.NewGuid(),
                AccountNumber = "JO-222222",
                Balance = 500,
                Currency = "JOD",
                CreatedAt = DateTime.UtcNow
            });

        db.Transactions.AddRange(
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Type = "Deposit",
                Amount = 100,
                Currency = "JOD",
                CreatedAt = DateTime.UtcNow
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = anotherAccountId,
                Type = "Withdrawal",
                Amount = 50,
                Currency = "JOD",
                CreatedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        var contextMock = CreateContextMock(db);

        var service = new TransactionService(
            contextMock.Object);

        // Act
        var result = await service.GetTransactionsByAccountIdAsync(
            accountId,
            1,
            10);

        // Assert
        Assert.Single(result);
        Assert.Equal("Deposit", result[0].Type);
        Assert.Equal(accountId, result[0].AccountId);
    }
}