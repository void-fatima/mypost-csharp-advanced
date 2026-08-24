using System.Security.Claims;
using MyPost.Application.Common;

namespace MyPost.Api.Infrastructure;

internal static class ClaimsPrincipalExtensions
{
    public static Guid UserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        return Guid.TryParse(value, out var userId) ? userId : throw new ForbiddenException("The access token does not identify a valid user.");
    }
}
