using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPost.Domain.Addresses;
using MyPost.Domain.Shipments;
using MyPost.Domain.Users;
using MyPost.Infrastructure.Identity;

namespace MyPost.Infrastructure.Persistence;

public sealed class DevelopmentDataSeeder(
    MyPostDbContext dbContext,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IConfiguration configuration,
    ILogger<DevelopmentDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var password = configuration["Seed:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Development seed skipped because Seed:Password is not configured.");
            return;
        }

        foreach (var role in Enum.GetNames<UserRole>())
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));

        var admin = await EnsureUserAsync("admin@mypost.local", "Ava Rahimi", UserRole.Admin, password);
        var courier = await EnsureUserAsync("courier@mypost.local", "Arman Karimi", UserRole.Courier, password);
        var customer = await EnsureUserAsync("customer@mypost.local", "Sara Ahmadi", UserRole.Customer, password);
        var secondCustomer = await EnsureUserAsync("customer2@mypost.local", "Nima Moradi", UserRole.Customer, password);

        if (await dbContext.Shipments.AnyAsync(cancellationToken)) return;

        var customerHome = new Address(customer.Id, "Home", "12 Valiasr Street", "Tehran", "Tehran", "1599911111", true);
        var secondHome = new Address(secondCustomer.Id, "Studio", "8 Eram Boulevard", "Shiraz", "Fars", "7188811111", true);
        dbContext.Addresses.AddRange(customerHome, secondHome);

        var now = DateTimeOffset.UtcNow;
        var shipments = new[]
        {
            Create("MP-DEMO-100001", customer, customerHome, ShipmentStatus.AwaitingPickup, now.AddDays(-1)),
            Create("MP-DEMO-100002", customer, customerHome, ShipmentStatus.InTransit, now.AddDays(-3), courier, admin),
            Create("MP-DEMO-100003", customer, customerHome, ShipmentStatus.OutForDelivery, now.AddDays(-2), courier, admin),
            Create("MP-DEMO-100004", customer, customerHome, ShipmentStatus.Delivered, now.AddDays(-7), courier, admin),
            Create("MP-DEMO-100005", secondCustomer, secondHome, ShipmentStatus.DeliveryFailed, now.AddDays(-4), courier, admin),
            Create("MP-DEMO-100006", secondCustomer, secondHome, ShipmentStatus.ReturningToSender, now.AddDays(-8), courier, admin)
        };
        dbContext.Shipments.AddRange(shipments);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Development-only MyPost users and shipment scenarios were seeded.");
    }

    private async Task<AppUser> EnsureUserAsync(string email, string displayName, UserRole role, string password)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null) return existing;
        var user = new AppUser { Id = Guid.NewGuid(), Email = email, UserName = email, DisplayName = displayName, EmailConfirmed = true, CreatedAtUtc = DateTimeOffset.UtcNow };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        await userManager.AddToRoleAsync(user, role.ToString());
        return user;
    }

    private static Shipment Create(string code, AppUser customer, Address sender, ShipmentStatus target, DateTimeOffset created, AppUser? courier = null, AppUser? admin = null)
    {
        var shipment = new Shipment(
            customer.Id,
            code,
            sender.Snapshot(),
            "Demo Recipient",
            "09120000000",
            new AddressSnapshot("Destination", "24 Demo Avenue", "Isfahan", "Isfahan", "8146811111"),
            ShipmentType.Parcel,
            1_250,
            new Dimensions(30, 20, 12),
            ServiceLevel.Standard,
            185_000,
            created);
        if (courier is not null && admin is not null) shipment.AssignCourier(courier.Id, admin.Id, created.AddHours(1));
        Advance(shipment, target, created, courier?.Id, admin?.Id ?? customer.Id);
        return shipment;
    }

    private static void Advance(Shipment shipment, ShipmentStatus target, DateTimeOffset created, Guid? courierId, Guid actorId)
    {
        if (target == ShipmentStatus.Created) return;
        shipment.TransitionTo(ShipmentStatus.AwaitingPickup, created.AddHours(1), actorId, "Ready for postal acceptance", "Tehran hub");
        if (target == ShipmentStatus.AwaitingPickup) return;
        shipment.TransitionTo(ShipmentStatus.Accepted, created.AddHours(3), actorId, "Accepted at origin facility", "Tehran hub");
        if (target == ShipmentStatus.Accepted) return;
        shipment.TransitionTo(ShipmentStatus.InTransit, created.AddHours(8), actorId, "Departed origin facility", "Tehran hub");
        if (target == ShipmentStatus.InTransit) return;
        shipment.TransitionTo(ShipmentStatus.OutForDelivery, created.AddDays(1), actorId, "Out for delivery", "Isfahan");
        if (target == ShipmentStatus.OutForDelivery) return;
        if (target == ShipmentStatus.Delivered)
        {
            shipment.RecordDelivery(DeliveryResult.Delivered, "Delivered to recipient", created.AddDays(1).AddHours(3), courierId ?? actorId);
            return;
        }
        shipment.RecordDelivery(DeliveryResult.RecipientUnavailable, "Recipient unavailable", created.AddDays(1).AddHours(3), courierId ?? actorId);
        if (target == ShipmentStatus.DeliveryFailed) return;
        shipment.InitiateReturn(created.AddDays(2), actorId, "Delivery attempts exhausted");
        if (target == ShipmentStatus.ReturnInitiated) return;
        shipment.TransitionTo(ShipmentStatus.ReturningToSender, created.AddDays(3), actorId, "Returning to sender", "Isfahan hub");
        if (target == ShipmentStatus.ReturningToSender) return;
        shipment.TransitionTo(ShipmentStatus.ReturnedToSender, created.AddDays(4), actorId, "Returned to sender", "Tehran");
    }
}
