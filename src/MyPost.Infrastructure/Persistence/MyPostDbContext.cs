using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPost.Application.Abstractions;
using MyPost.Domain.Addresses;
using MyPost.Domain.Shipments;
using MyPost.Infrastructure.Identity;

namespace MyPost.Infrastructure.Persistence;

public sealed class MyPostDbContext(DbContextOptions<MyPostDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();
    public DbSet<CourierAssignment> CourierAssignments => Set<CourierAssignment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("mypost");

        builder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(user => user.DisplayName).HasMaxLength(160).IsRequired();
            entity.HasIndex(user => user.NormalizedEmail).IsUnique();
        });
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UserId, token.ExpiresAtUtc });
            entity.HasOne(token => token.User).WithMany(user => user.RefreshTokens).HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            entity.HasKey(address => address.Id);
            entity.Property(address => address.Label).HasMaxLength(80).IsRequired();
            entity.Property(address => address.Line1).HasMaxLength(240).IsRequired();
            entity.Property(address => address.City).HasMaxLength(100).IsRequired();
            entity.Property(address => address.Province).HasMaxLength(100).IsRequired();
            entity.Property(address => address.PostalCode).HasMaxLength(20).IsRequired();
            entity.HasIndex(address => address.OwnerUserId);
            entity.HasIndex(address => new { address.OwnerUserId, address.PostalCode });
        });

        builder.Entity<Shipment>(entity =>
        {
            entity.ToTable("Shipments");
            entity.HasKey(shipment => shipment.Id);
            entity.Property(shipment => shipment.TrackingCode).HasMaxLength(32).IsRequired();
            entity.Property(shipment => shipment.RecipientName).HasMaxLength(160).IsRequired();
            entity.Property(shipment => shipment.RecipientPhone).HasMaxLength(30).IsRequired();
            entity.Property(shipment => shipment.CalculatedPrice).HasPrecision(18, 2);
            entity.Property(shipment => shipment.WeightGrams).HasPrecision(12, 2);
            entity.Property(shipment => shipment.DeliveryNote).HasMaxLength(500);
            entity.Property(shipment => shipment.CustomerReference).HasMaxLength(100);
            entity.Property(shipment => shipment.Version).IsConcurrencyToken();
            entity.HasIndex(shipment => shipment.TrackingCode).IsUnique();
            entity.HasIndex(shipment => new { shipment.SenderUserId, shipment.CreatedAtUtc });
            entity.HasIndex(shipment => new { shipment.CourierUserId, shipment.Status });
            entity.HasIndex(shipment => new { shipment.SenderUserId, shipment.CustomerReference })
                .IsUnique()
                .HasFilter("\"CustomerReference\" IS NOT NULL");

            entity.OwnsOne(shipment => shipment.SenderAddress, owned => ConfigureAddressSnapshot(owned, "Sender"));
            entity.OwnsOne(shipment => shipment.DestinationAddress, owned => ConfigureAddressSnapshot(owned, "Destination"));
            entity.OwnsOne(shipment => shipment.Dimensions, owned =>
            {
                owned.Property(value => value.LengthCm).HasColumnName("LengthCm").HasPrecision(8, 2);
                owned.Property(value => value.WidthCm).HasColumnName("WidthCm").HasPrecision(8, 2);
                owned.Property(value => value.HeightCm).HasColumnName("HeightCm").HasPrecision(8, 2);
            });

            entity.HasMany(shipment => shipment.TrackingEvents)
                .WithOne()
                .HasForeignKey(item => item.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(shipment => shipment.TrackingEvents).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasMany(shipment => shipment.Assignments)
                .WithOne()
                .HasForeignKey(item => item.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(shipment => shipment.Assignments).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<TrackingEvent>(entity =>
        {
            entity.ToTable("TrackingEvents");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Description).HasMaxLength(300).IsRequired();
            entity.Property(item => item.Location).HasMaxLength(160);
            entity.HasIndex(item => new { item.ShipmentId, item.OccurredAtUtc });
        });

        builder.Entity<CourierAssignment>(entity =>
        {
            entity.ToTable("CourierAssignments");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.CourierUserId, item.EndedAtUtc });
            entity.HasIndex(item => new { item.ShipmentId, item.EndedAtUtc });
        });
    }

    private static void ConfigureAddressSnapshot<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.OwnedNavigationBuilder<T, AddressSnapshot> owned, string prefix)
        where T : class
    {
        owned.Property(value => value.Label).HasColumnName($"{prefix}Label").HasMaxLength(80).IsRequired();
        owned.Property(value => value.Line1).HasColumnName($"{prefix}Line1").HasMaxLength(240).IsRequired();
        owned.Property(value => value.City).HasColumnName($"{prefix}City").HasMaxLength(100).IsRequired();
        owned.Property(value => value.Province).HasColumnName($"{prefix}Province").HasMaxLength(100).IsRequired();
        owned.Property(value => value.PostalCode).HasColumnName($"{prefix}PostalCode").HasMaxLength(20).IsRequired();
        owned.Property(value => value.Country).HasColumnName($"{prefix}Country").HasMaxLength(80).IsRequired();
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);
}
