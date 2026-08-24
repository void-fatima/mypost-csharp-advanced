using MyPost.Application.Common;
using MyPost.Domain.Users;

namespace MyPost.Application.Users;

public sealed record UserSummaryDto(Guid Id, string Email, string DisplayName, UserRole Role, bool IsActive);

public interface IUserDirectory
{
    Task<bool> IsInRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default);
    Task<PagedResult<UserSummaryDto>> ListAsync(PageRequest page, string? search, CancellationToken cancellationToken = default);
}
