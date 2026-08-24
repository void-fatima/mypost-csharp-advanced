using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPost.Application.Abstractions;
using MyPost.Application.Addresses;
using MyPost.Application.Operations;
using MyPost.Application.Shipments;
using MyPost.Application.Users;
using MyPost.Infrastructure.Identity;
using MyPost.Infrastructure.Operations;
using MyPost.Infrastructure.Persistence;

namespace MyPost.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMyPostInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MyPost")
            ?? throw new InvalidOperationException("ConnectionStrings:MyPost must be configured.");

        services.AddDbContext<MyPostDbContext>(options => options.UseNpgsql(connectionString));
        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<MyPostDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<AuthService>();
        services.AddScoped<DevelopmentDataSeeder>();

        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<MyPostDbContext>());
        services.AddScoped<IOperationsReadService, OperationsReadService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ITrackingCodeGenerator, TrackingCodeGenerator>();
        services.AddSingleton<IShipmentPriceCalculator, ShipmentPriceCalculator>();
        services.AddScoped<AddressService>();
        services.AddScoped<ShipmentService>();
        return services;
    }
}
