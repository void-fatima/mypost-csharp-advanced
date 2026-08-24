using MyPost.Domain.Users;

namespace MyPost.Infrastructure.Identity;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record UserProfile(Guid Id, string Email, string DisplayName, UserRole Role);
public sealed record AccessTokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, UserProfile User);
public sealed record AuthenticationSession(AccessTokenResponse Response, string RefreshToken, DateTimeOffset RefreshExpiresAtUtc);
public sealed record JwtOptions(string Issuer, string Audience, string SigningKey, int AccessTokenMinutes = 10, int RefreshTokenDays = 7)
{
    public const string SectionName = "Jwt";
}
