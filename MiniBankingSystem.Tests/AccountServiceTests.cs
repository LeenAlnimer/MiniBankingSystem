using Microsoft.EntityFrameworkCore;
using Moq;
using MiniBankingSystem.Application.DTOs;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Application.Services;
using MiniBankingSystem.Domain.Entities;

namespace MiniBankingSystem.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task WithdrawAsync_ShouldReduceAccountBalance()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-123456",
            Balance = 1000,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new WithdrawDto
        {
            AccountId = accountId,
            Amount = 300
        };

        // Act
        var result = await service.WithdrawAsync(dto);

        // Assert
        Assert.Equal(700, result.Balance);
        Assert.Equal(700, account.Balance);

        cacheMock.Verify(
            x => x.RemoveAsync($"account:{accountId}"),
            Times.Once);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldThrowException_WhenBalanceIsInsufficient()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-654321",
            Balance = 100,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new WithdrawDto
        {
            AccountId = accountId,
            Amount = 500
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.WithdrawAsync(dto));

        Assert.Equal("Insufficient balance.", exception.Message);

        Assert.Equal(100, account.Balance);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldThrowException_WhenAmountIsInvalid()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-111111",
            Balance = 1000,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new WithdrawDto
        {
            AccountId = accountId,
            Amount = 0
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.WithdrawAsync(dto));

        Assert.Equal(
            "Withdrawal amount must be greater than zero.",
            exception.Message);

        Assert.Equal(1000, account.Balance);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldThrowException_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new WithdrawDto
        {
            AccountId = accountId,
            Amount = 100
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.WithdrawAsync(dto));

        Assert.Equal("Account not found.", exception.Message);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task DepositAsync_ShouldIncreaseAccountBalance()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-222222",
            Balance = 1000,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new DepositDto
        {
            AccountId = accountId,
            Amount = 500
        };

        // Act
        var result = await service.DepositAsync(dto);

        // Assert
        Assert.Equal(1500, result.Balance);
        Assert.Equal(1500, account.Balance);

        var transaction = await db.Transactions
            .FirstOrDefaultAsync();

        Assert.NotNull(transaction);
        Assert.Equal(accountId, transaction.AccountId);
        Assert.Equal("Deposit", transaction.Type);
        Assert.Equal(500, transaction.Amount);
        Assert.Equal("JOD", transaction.Currency);

        cacheMock.Verify(
            x => x.RemoveAsync($"account:{accountId}"),
            Times.Once);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task DepositAsync_ShouldThrowException_WhenAmountIsInvalid()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-333333",
            Balance = 1000,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new DepositDto
        {
            AccountId = accountId,
            Amount = 0
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.DepositAsync(dto));

        Assert.Equal(
            "Deposit amount must be greater than zero.",
            exception.Message);

        Assert.Equal(1000, account.Balance);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task DepositAsync_ShouldThrowException_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new DepositDto
        {
            AccountId = accountId,
            Amount = 500
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.DepositAsync(dto));

        Assert.Equal("Account not found.", exception.Message);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ShouldTransferMoneySuccessfully()
    {
        // Arrange
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-444444",
            Balance = 1000,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-555555",
            Balance = 500,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.AddRange(fromAccount, toAccount);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new TransferDto
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = 300
        };

        // Act
        await service.TransferAsync(dto);

        // Assert
        Assert.Equal(700, fromAccount.Balance);
        Assert.Equal(800, toAccount.Balance);

        var transactions = await db.Transactions
            .ToListAsync();

        Assert.Equal(2, transactions.Count);

        var withdrawalTransaction = transactions
            .FirstOrDefault(x => x.Type == "Transfer Out");

        var depositTransaction = transactions
            .FirstOrDefault(x => x.Type == "Transfer In");

        Assert.NotNull(withdrawalTransaction);
        Assert.NotNull(depositTransaction);

        Assert.Equal(fromAccountId, withdrawalTransaction.AccountId);
        Assert.Equal(toAccountId, depositTransaction.AccountId);

        Assert.Equal(300, withdrawalTransaction.Amount);
        Assert.Equal(300, depositTransaction.Amount);

        Assert.Equal("JOD", withdrawalTransaction.Currency);
        Assert.Equal("JOD", depositTransaction.Currency);

        cacheMock.Verify(
            x => x.RemoveAsync($"account:{fromAccountId}"),
            Times.Once);

        cacheMock.Verify(
            x => x.RemoveAsync($"account:{toAccountId}"),
            Times.Once);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task TransferAsync_ShouldThrowException_WhenSourceAccountDoesNotExist()
    {
        // Arrange
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var toAccount = new Account
        {
            Id = toAccountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-666666",
            Balance = 500,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.Add(toAccount);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new TransferDto
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = 200
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.TransferAsync(dto));

        Assert.Equal(
            "Source account not found.",
            exception.Message);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ShouldThrowException_WhenDestinationAccountDoesNotExist()
    {
        // Arrange
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-777777",
            Balance = 1000,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.Add(fromAccount);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new TransferDto
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = 200
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.TransferAsync(dto));

        Assert.Equal(
            "Destination account not found.",
            exception.Message);

        Assert.Equal(1000, fromAccount.Balance);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ShouldThrowException_WhenSourceAndDestinationAreSame()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-888888",
            Balance = 1000,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new TransferDto
        {
            FromAccountId = accountId,
            ToAccountId = accountId,
            Amount = 200
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.TransferAsync(dto));

        Assert.Equal(
            "Source and destination accounts must be different.",
            exception.Message);

        Assert.Equal(1000, account.Balance);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ShouldThrowException_WhenBalanceIsInsufficient()
    {
        // Arrange
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-999999",
            Balance = 100,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-101010",
            Balance = 500,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.AddRange(fromAccount, toAccount);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new TransferDto
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = 300
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TransferAsync(dto));

        Assert.Equal(
            "Insufficient balance.",
            exception.Message);

        Assert.Equal(100, fromAccount.Balance);
        Assert.Equal(500, toAccount.Balance);

        var transactions = await db.Transactions.ToListAsync();

        Assert.Empty(transactions);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task TransferAsync_ShouldThrowException_WhenCurrenciesAreDifferent()
    {
        // Arrange
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-121212",
            Balance = 1000,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "USD-343434",
            Balance = 500,
            Currency = "USD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.AddRange(fromAccount, toAccount);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        contextMock
            .Setup(x => x.Transactions)
            .Returns(db.Transactions);

        contextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => db.SaveChangesAsync());

        var cacheMock = new Mock<ICacheService>();
        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        var dto = new TransferDto
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = 300
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TransferAsync(dto));

        Assert.Equal(
            "Accounts must use the same currency.",
            exception.Message);

        Assert.Equal(1000, fromAccount.Balance);
        Assert.Equal(500, toAccount.Balance);

        var transactions = await db.Transactions.ToListAsync();

        Assert.Empty(transactions);

        cacheMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.Never);

        rabbitMqMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAccountByIdAsync_ShouldReturnAccountFromDatabase_WhenCacheMisses()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-141414",
            Balance = 1500,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        var cacheMock = new Mock<ICacheService>();

        cacheMock
            .Setup(x => x.GetAsync($"account:{accountId}"))
            .ReturnsAsync((string?)null);

        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        // Act
        var result = await service.GetAccountByIdAsync(accountId);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(accountId, result.Id);
        Assert.Equal("JO-141414", result.AccountNumber);
        Assert.Equal(1500, result.Balance);
        Assert.Equal("JOD", result.Currency);

        cacheMock.Verify(
            x => x.GetAsync($"account:{accountId}"),
            Times.Once);

        cacheMock.Verify(
            x => x.SetAsync(
                $"account:{accountId}",
                It.IsAny<string>(),
                TimeSpan.FromMinutes(5)),
            Times.Once);
    }

    [Fact]
    public async Task GetAccountByIdAsync_ShouldReturnAccountFromCache_WhenCacheHit()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var cachedAccount = new AccountDto
        {
            Id = accountId,
            CustomerId = Guid.NewGuid(),
            AccountNumber = "JO-151515",
            Balance = 2500,
            Currency = "JOD",
            CreatedAt = DateTime.UtcNow
        };

        var cachedJson =
            System.Text.Json.JsonSerializer.Serialize(cachedAccount);

        var contextMock = new Mock<IApplicationDbContext>();

        var cacheMock = new Mock<ICacheService>();

        cacheMock
            .Setup(x => x.GetAsync($"account:{accountId}"))
            .ReturnsAsync(cachedJson);

        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        // Act
        var result = await service.GetAccountByIdAsync(accountId);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(accountId, result.Id);
        Assert.Equal("JO-151515", result.AccountNumber);
        Assert.Equal(2500, result.Balance);
        Assert.Equal("JOD", result.Currency);

        cacheMock.Verify(
            x => x.GetAsync($"account:{accountId}"),
            Times.Once);

        cacheMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>()),
            Times.Never);

        contextMock.Verify(
            x => x.Accounts,
            Times.Never);
    }

    [Fact]
    public async Task GetAccountByIdAsync_ShouldReturnNull_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Accounts)
            .Returns(db.Accounts);

        var cacheMock = new Mock<ICacheService>();

        cacheMock
            .Setup(x => x.GetAsync($"account:{accountId}"))
            .ReturnsAsync((string?)null);

        var rabbitMqMock = new Mock<IRabbitMqService>();

        var service = new AccountService(
            contextMock.Object,
            cacheMock.Object,
            rabbitMqMock.Object);

        // Act
        var result = await service.GetAccountByIdAsync(accountId);

        // Assert
        Assert.Null(result);

        cacheMock.Verify(
            x => x.GetAsync($"account:{accountId}"),
            Times.Once);

        cacheMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>()),
            Times.Never);
    }
}