using Microsoft.AspNetCore.Identity;

namespace MyPost.Infrastructure.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
