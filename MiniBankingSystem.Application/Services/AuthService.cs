using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Domain.Entities;

namespace MiniBankingSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<string> RegisterAsync(
        string username,
        string email,
        string password)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == username ||
                u.Email == email);

        if (existingUser != null)
        {
            throw new ArgumentException(
                "Username or email already exists.");
        }

        var passwordHash =
            _passwordHasher.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return "User registered successfully.";
    }

    public async Task<string> LoginAsync(
        string username,
        string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == username);

        if (user == null)
        {
            throw new ArgumentException(
                "Invalid username or password.");
        }

        var isPasswordValid =
            _passwordHasher.VerifyPassword(
                password,
                user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new ArgumentException(
                "Invalid username or password.");
        }

        var token = _jwtTokenService.GenerateToken(
            user.Id,
            user.Username,
            user.Email,
            user.Role);

        return token;
    }
}