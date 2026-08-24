using MyPost.Api.Infrastructure;
using MyPost.Infrastructure.Identity;

namespace MyPost.Api.Endpoints;

internal static class AuthEndpoints
{
    private const string RefreshCookie = "mypost_refresh";

    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/auth").WithTags("Authentication").RequireRateLimiting("auth");
        group.MapPost("/register", async (RegisterRequest request, AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            var session = await auth.RegisterAsync(request, cancellationToken);
            SetCookie(context, session);
            return Results.Ok(session.Response);
        });
        group.MapPost("/login", async (LoginRequest request, AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            var session = await auth.LoginAsync(request, cancellationToken);
            SetCookie(context, session);
            return Results.Ok(session.Response);
        });
        group.MapPost("/refresh", async (AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            var token = context.Request.Cookies[RefreshCookie] ?? string.Empty;
            var session = await auth.RefreshAsync(token, cancellationToken);
            SetCookie(context, session);
            return Results.Ok(session.Response);
        });
        group.MapPost("/logout", async (AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            await auth.LogoutAsync(context.Request.Cookies[RefreshCookie], context.User.UserId(), cancellationToken);
            context.Response.Cookies.Delete(RefreshCookie, CookieOptions(context));
            return Results.NoContent();
        }).RequireAuthorization();
        group.MapGet("/me", async (AuthService auth, HttpContext context) => Results.Ok(await auth.ProfileAsync(context.User.UserId())))
            .RequireAuthorization();
        return api;
    }

    private static void SetCookie(HttpContext context, AuthenticationSession session) =>
        context.Response.Cookies.Append(RefreshCookie, session.RefreshToken, CookieOptions(context, session.RefreshExpiresAtUtc));

    private static CookieOptions CookieOptions(HttpContext context, DateTimeOffset? expires = null) => new()
    {
        HttpOnly = true,
        Secure = !context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = "/api/v1/auth",
        Expires = expires,
        IsEssential = true
    };
}
