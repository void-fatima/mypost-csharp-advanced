using MyPost.Application.Shipments;

namespace MyPost.Api.Endpoints;

internal static class PublicEndpoints
{
    public static RouteGroupBuilder MapPublicEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/tracking/{trackingCode}", async (string trackingCode, ShipmentService service, CancellationToken token) =>
                Results.Ok(await service.TrackPublicAsync(trackingCode, token)))
            .WithTags("Public tracking")
            .RequireRateLimiting("public-tracking");
        return api;
    }
}
