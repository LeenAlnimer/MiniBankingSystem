namespace MiniBankingSystem.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(
        Guid userId,
        string username,
        string email,
        string role);
}