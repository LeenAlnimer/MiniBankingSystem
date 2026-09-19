using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Application.Services;
using MiniBankingSystem.Domain.Entities;
using Moq;

namespace MiniBankingSystem.Tests;

public class AuthServiceTests
{
    private static Mock<IApplicationDbContext> CreateContextMock(
        TestDbContext db)
    {
        var contextMock = new Mock<IApplicationDbContext>();

        contextMock
            .Setup(x => x.Users)
            .Returns(db.Users);

        contextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken _) =>
            {
                return await db.SaveChangesAsync();
            });

        return contextMock;
    }

    [Fact]
    public async Task RegisterAsync_ShouldRegisterUserSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        passwordHasherMock
            .Setup(x => x.HashPassword("Password123"))
            .Returns("hashed-password");

        var contextMock = CreateContextMock(db);

        var service = new AuthService(
            contextMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object);

        // Act
        var result = await service.RegisterAsync(
            "leen",
            "leen@example.com",
            "Password123");

        // Assert
        Assert.Equal(
            "User registered successfully.",
            result);

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Username == "leen");

        Assert.NotNull(user);
        Assert.Equal("leen@example.com", user.Email);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal("Customer", user.Role);

        passwordHasherMock.Verify(
            x => x.HashPassword("Password123"),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        passwordHasherMock
            .Setup(x => x.HashPassword("MyPassword"))
            .Returns("secure-hashed-password");

        var contextMock = CreateContextMock(db);

        var service = new AuthService(
            contextMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object);

        // Act
        await service.RegisterAsync(
            "leen",
            "leen@example.com",
            "MyPassword");

        // Assert
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Username == "leen");

        Assert.NotNull(user);

        Assert.Equal(
            "secure-hashed-password",
            user.PasswordHash);

        Assert.NotEqual(
            "MyPassword",
            user.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenUsernameAlreadyExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Username = "leen",
            Email = "old@example.com",
            PasswordHash = "old-hash",
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        var contextMock = CreateContextMock(db);

        var service = new AuthService(
            contextMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.RegisterAsync(
                    "leen",
                    "new@example.com",
                    "Password123"));

        Assert.Equal(
            "Username or email already exists.",
            exception.Message);

        passwordHasherMock.Verify(
            x => x.HashPassword(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Username = "olduser",
            Email = "leen@example.com",
            PasswordHash = "old-hash",
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        var contextMock = CreateContextMock(db);

        var service = new AuthService(
            contextMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.RegisterAsync(
                    "newuser",
                    "leen@example.com",
                    "Password123"));

        Assert.Equal(
            "Username or email already exists.",
            exception.Message);

        passwordHasherMock.Verify(
            x => x.HashPassword(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var userId = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = userId,
            Username = "leen",
            Email = "leen@example.com",
            PasswordHash = "hashed-password",
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        passwordHasherMock
            .Setup(x => x.VerifyPassword(
                "Password123",
                "hashed-password"))
            .Returns(true);

        jwtTokenServiceMock
            .Setup(x => x.GenerateToken(
                userId,
                "leen",
                "leen@example.com",
                "Customer"))
            .Returns("fake-jwt-token");

        var contextMock = CreateContextMock(db);

        var service = new AuthService(
            contextMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object);

        // Act
        var result = await service.LoginAsync(
            "leen",
            "Password123");

        // Assert
        Assert.Equal("fake-jwt-token", result);

        passwordHasherMock.Verify(
            x => x.VerifyPassword(
                "Password123",
                "hashed-password"),
            Times.Once);

        jwtTokenServiceMock.Verify(
            x => x.GenerateToken(
                userId,
                "leen",
                "leen@example.com",
                "Customer"),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenUsernameDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        var contextMock = CreateContextMock(db);

        var service = new AuthService(
            contextMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.LoginAsync(
                    "unknown-user",
                    "Password123"));

        Assert.Equal(
            "Invalid username or password.",
            exception.Message);

        passwordHasherMock.Verify(
            x => x.VerifyPassword(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        jwtTokenServiceMock.Verify(
            x => x.GenerateToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenPasswordIsInvalid()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var userId = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = userId,
            Username = "leen",
            Email = "leen@example.com",
            PasswordHash = "correct-hash",
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        passwordHasherMock
            .Setup(x => x.VerifyPassword(
                "WrongPassword",
                "correct-hash"))
            .Returns(false);

        var contextMock = CreateContextMock(db);

        var service = new AuthService(
            contextMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.LoginAsync(
                    "leen",
                    "WrongPassword"));

        Assert.Equal(
            "Invalid username or password.",
            exception.Message);

        passwordHasherMock.Verify(
            x => x.VerifyPassword(
                "WrongPassword",
                "correct-hash"),
            Times.Once);

        jwtTokenServiceMock.Verify(
            x => x.GenerateToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldGenerateToken_WithCorrectUserInformation()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new TestDbContext(options);

        var userId = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = userId,
            Username = "leen",
            Email = "leen@example.com",
            PasswordHash = "hashed-password",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        passwordHasherMock
            .Setup(x => x.VerifyPassword(
                "Password123",
                "hashed-password"))
            .Returns(true);

        jwtTokenServiceMock
            .Setup(x => x.GenerateToken(
                userId,
                "leen",
                "leen@example.com",
                "Admin"))
            .Returns("admin-jwt-token");

        var contextMock = CreateContextMock(db);

        var service = new AuthService(
            contextMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object);

        // Act
        var result = await service.LoginAsync(
            "leen",
            "Password123");

        // Assert
        Assert.Equal("admin-jwt-token", result);

        jwtTokenServiceMock.Verify(
            x => x.GenerateToken(
                userId,
                "leen",
                "leen@example.com",
                "Admin"),
            Times.Once);
    }
}