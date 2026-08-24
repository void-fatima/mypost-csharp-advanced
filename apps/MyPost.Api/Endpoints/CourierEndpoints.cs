using MyPost.Api.Infrastructure;
using MyPost.Application.Common;
using MyPost.Application.Shipments;
using MyPost.Domain.Shipments;

namespace MyPost.Api.Endpoints;

internal static class CourierEndpoints
{
    public static RouteGroupBuilder MapCourierEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/courier").WithTags("Courier").RequireAuthorization("Courier");
        group.MapGet("/shipments", async (int? page, int? pageSize, ShipmentStatus? status, string? search, ShipmentService service, HttpContext context, CancellationToken token) =>
            Results.Ok(await service.ListAssignedAsync(context.User.UserId(), new ShipmentFilter(page ?? 1, pageSize ?? 20, status, search), token)));
        group.MapGet("/shipments/{id:guid}", async (Guid id, ShipmentService service, HttpContext context, CancellationToken token) =>
            Results.Ok(await service.GetAssignedAsync(context.User.UserId(), id, token)));
        group.MapPost("/shipments/{id:guid}/status", async (Guid id, TransitionShipmentRequest request, ShipmentService service, HttpContext context, CancellationToken token) =>
        {
            if (request.Status is not (ShipmentStatus.InTransit or ShipmentStatus.OutForDelivery))
                throw new ValidationException(new Dictionary<string, string[]> { ["status"] = ["Couriers may update only in-transit and out-for-delivery states here."] });
            await service.GetAssignedAsync(context.User.UserId(), id, token);
            await service.TransitionAsync(id, request, context.User.UserId(), token);
            return Results.NoContent();
        });
        group.MapPost("/shipments/{id:guid}/delivery", async (Guid id, RecordDeliveryRequest request, ShipmentService service, HttpContext context, CancellationToken token) =>
        {
            await service.RecordDeliveryAsync(id, context.User.UserId(), request, token);
            return Results.NoContent();
        });
        return api;
    }
}
