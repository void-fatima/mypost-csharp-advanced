using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPost.Application.Common;
using MyPost.Application.Users;
using MyPost.Domain.Users;
using MyPost.Infrastructure.Persistence;

namespace MyPost.Infrastructure.Identity;

internal sealed class UserDirectory(UserManager<AppUser> userManager, MyPostDbContext dbContext) : IUserDirectory
{
    public async Task<bool> IsInRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is { IsActive: true } && await userManager.IsInRoleAsync(user, role.ToString());
    }

    public async Task<PagedResult<UserSummaryDto>> ListAsync(PageRequest page, string? search, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user => user.DisplayName.Contains(term) || (user.Email != null && user.Email.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var users = await query.OrderBy(user => user.DisplayName)
            .Skip((page.SafePage - 1) * page.SafePageSize)
            .Take(page.SafePageSize)
            .ToListAsync(cancellationToken);
        var result = new List<UserSummaryDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var role = roles.Select(value => Enum.TryParse<UserRole>(value, out var parsed) ? parsed : UserRole.Customer).FirstOrDefault();
            result.Add(new UserSummaryDto(user.Id, user.Email ?? string.Empty, user.DisplayName, role, user.IsActive));
        }

        return new PagedResult<UserSummaryDto>(result, page.SafePage, page.SafePageSize, total);
    }
}
