using MyPost.Api.Infrastructure;
using MyPost.Application.Addresses;
using MyPost.Application.Shipments;
using MyPost.Domain.Shipments;

namespace MyPost.Api.Endpoints;

internal static class CustomerEndpoints
{
    public static RouteGroupBuilder MapCustomerEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/customer").WithTags("Customer").RequireAuthorization("Customer");
        group.MapGet("/addresses", async (AddressService service, HttpContext context, CancellationToken token) => Results.Ok(await service.ListAsync(context.User.UserId(), token)));
        group.MapPost("/addresses", async (UpsertAddressRequest request, AddressService service, HttpContext context, CancellationToken token) =>
            Results.Created("/api/v1/customer/addresses", await service.CreateAsync(context.User.UserId(), request, token)));
        group.MapPut("/addresses/{id:guid}", async (Guid id, UpsertAddressRequest request, AddressService service, HttpContext context, CancellationToken token) =>
            Results.Ok(await service.UpdateAsync(context.User.UserId(), id, request, token)));
        group.MapDelete("/addresses/{id:guid}", async (Guid id, AddressService service, HttpContext context, CancellationToken token) =>
        {
            await service.DeleteAsync(context.User.UserId(), id, token);
            return Results.NoContent();
        });
        group.MapPost("/shipments", async (CreateShipmentRequest request, ShipmentService service, HttpContext context, CancellationToken token) =>
        {
            var shipment = await service.CreateAsync(context.User.UserId(), request, token);
            return Results.Created($"/api/v1/customer/shipments/{shipment.Id}", shipment);
        });
        group.MapGet("/shipments", async (int? page, int? pageSize, ShipmentStatus? status, string? search, ShipmentService service, HttpContext context, CancellationToken token) =>
            Results.Ok(await service.ListOwnedAsync(context.User.UserId(), new ShipmentFilter(page ?? 1, pageSize ?? 20, status, search), token)));
        group.MapGet("/shipments/{id:guid}", async (Guid id, ShipmentService service, HttpContext context, CancellationToken token) =>
            Results.Ok(await service.GetOwnedAsync(context.User.UserId(), id, token)));
        group.MapPost("/shipments/{id:guid}/cancel", async (Guid id, ShipmentService service, HttpContext context, CancellationToken token) =>
        {
            await service.CancelAsync(context.User.UserId(), id, token);
            return Results.NoContent();
        });
        return api;
    }
}
