using MyPost.Api.Infrastructure;
using MyPost.Application.Common;
using MyPost.Application.Operations;
using MyPost.Application.Shipments;
using MyPost.Application.Users;
using MyPost.Domain.Shipments;

namespace MyPost.Api.Endpoints;

internal static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/admin").WithTags("Administration").RequireAuthorization("Admin");
        group.MapGet("/overview", async (IOperationsReadService service, CancellationToken token) => Results.Ok(await service.GetOverviewAsync(token)));
        group.MapGet("/analytics/statuses", async (IOperationsReadService service, CancellationToken token) => Results.Ok(await service.GetStatusCountsAsync(token)));
        group.MapGet("/shipments", async (int? page, int? pageSize, ShipmentStatus? status, string? search, ShipmentService service, CancellationToken token) =>
            Results.Ok(await service.ListAllAsync(new ShipmentFilter(page ?? 1, pageSize ?? 20, status, search), token)));
        group.MapGet("/shipments/{id:guid}", async (Guid id, ShipmentService service, CancellationToken token) => Results.Ok(await service.GetAnyAsync(id, token)));
        group.MapPost("/shipments/{id:guid}/assign", async (Guid id, AssignCourierRequest request, ShipmentService service, HttpContext context, CancellationToken token) =>
        {
            await service.AssignCourierAsync(id, request.CourierUserId, context.User.UserId(), token);
            return Results.NoContent();
        });
        group.MapPost("/shipments/{id:guid}/status", async (Guid id, TransitionShipmentRequest request, ShipmentService service, HttpContext context, CancellationToken token) =>
        {
            await service.TransitionAsync(id, request, context.User.UserId(), token);
            return Results.NoContent();
        });
        group.MapPost("/shipments/{id:guid}/return", async (Guid id, InitiateReturnRequest request, ShipmentService service, HttpContext context, CancellationToken token) =>
        {
            await service.InitiateReturnAsync(id, context.User.UserId(), request.Reason, token);
            return Results.NoContent();
        });
        group.MapGet("/users", async (int? page, int? pageSize, string? search, IUserDirectory users, CancellationToken token) =>
            Results.Ok(await users.ListAsync(new PageRequest(page ?? 1, pageSize ?? 20), search, token)));
        return api;
    }
}
