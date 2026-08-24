using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyPost.Application.Abstractions;
using MyPost.Application.Common;
using MyPost.Domain.Users;
using MyPost.Infrastructure.Persistence;

namespace MyPost.Infrastructure.Identity;

public sealed class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    MyPostDbContext dbContext,
    IClock clock,
    IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public async Task<AuthenticationSession> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.DisplayName))
            throw new ValidationException(new Dictionary<string, string[]> { ["registration"] = ["Email and display name are required."] });

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            EmailConfirmed = true,
            CreatedAtUtc = clock.UtcNow
        };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ValidationException(result.Errors.GroupBy(error => error.Code).ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
        await userManager.AddToRoleAsync(user, UserRole.Customer.ToString());
        return await CreateSessionAsync(user, UserRole.Customer, cancellationToken);
    }

    public async Task<AuthenticationSession> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
            throw new ForbiddenException("Invalid email or password.");
        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded) throw new ForbiddenException("Invalid email or password.");
        return await CreateSessionAsync(user, await RoleAsync(user), cancellationToken);
    }

    public async Task<AuthenticationSession> RefreshAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var hash = Hash(rawToken);
        var current = await dbContext.RefreshTokens.Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken)
            ?? throw new ForbiddenException("Refresh session is invalid.");
        if (!current.IsActive(clock.UtcNow) || !current.User.IsActive)
            throw new ForbiddenException("Refresh session has expired or was revoked.");

        current.RevokedAtUtc = clock.UtcNow;
        var role = await RoleAsync(current.User);
        var next = await CreateSessionAsync(current.User, role, cancellationToken, save: false);
        current.ReplacedByTokenHash = Hash(next.RefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return next;
    }

    public async Task LogoutAsync(string? rawToken, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(rawToken))
        {
            var hash = Hash(rawToken);
            var token = await dbContext.RefreshTokens.SingleOrDefaultAsync(item => item.TokenHash == hash && item.UserId == userId, cancellationToken);
            if (token is not null) token.RevokedAtUtc ??= clock.UtcNow;
        }
        else
        {
            var activeTokens = await dbContext.RefreshTokens.Where(item => item.UserId == userId && item.RevokedAtUtc == null).ToListAsync(cancellationToken);
            foreach (var token in activeTokens) token.RevokedAtUtc = clock.UtcNow;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfile> ProfileAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new NotFoundException("User not found.");
        return Profile(user, await RoleAsync(user));
    }

    private async Task<AuthenticationSession> CreateSessionAsync(AppUser user, UserRole role, CancellationToken cancellationToken, bool save = true)
    {
        ValidateOptions();
        var expiresAt = clock.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            clock.UtcNow.UtcDateTime,
            expiresAt.UtcDateTime,
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var rawRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshExpiresAt = clock.UtcNow.AddDays(_options.RefreshTokenDays);
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Hash(rawRefreshToken),
            CreatedAtUtc = clock.UtcNow,
            ExpiresAtUtc = refreshExpiresAt
        });
        if (save) await dbContext.SaveChangesAsync(cancellationToken);
        return new AuthenticationSession(
            new AccessTokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, Profile(user, role)),
            rawRefreshToken,
            refreshExpiresAt);
    }

    private async Task<UserRole> RoleAsync(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return roles.Select(value => Enum.TryParse<UserRole>(value, out var parsed) ? parsed : UserRole.Customer).FirstOrDefault();
    }

    private static UserProfile Profile(AppUser user, UserRole role) => new(user.Id, user.Email ?? string.Empty, user.DisplayName, role);
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private void ValidateOptions()
    {
        if (_options.SigningKey.Length < 32) throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 characters.");
        if (string.IsNullOrWhiteSpace(_options.Issuer) || string.IsNullOrWhiteSpace(_options.Audience))
            throw new InvalidOperationException("JWT issuer and audience must be configured.");
    }
}
