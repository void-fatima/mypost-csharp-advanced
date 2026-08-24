using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MyPost.Infrastructure.Persistence;

namespace MyPost.IntegrationTests;

internal sealed class MyPostApiFactory : WebApplicationFactory<MyPost.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:MyPost"] = "Host=unused;Database=unused;Username=unused",
            ["Jwt:Issuer"] = "MyPost.Tests",
            ["Jwt:Audience"] = "MyPost.Web.Tests",
            ["Jwt:SigningKey"] = "test-only-signing-key-at-least-thirty-two-characters",
            ["Database:AutoMigrate"] = "false",
            ["Seed:Enabled"] = "false"
        }));
        builder.ConfigureServices(services =>
        {
            var databaseName = $"mypost-tests-{Guid.NewGuid():N}";
            services.RemoveAll<DbContextOptions<MyPostDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<MyPostDbContext>>();
            services.RemoveAll<IDatabaseProvider>();
            services.AddDbContext<MyPostDbContext>(options => options.UseInMemoryDatabase(databaseName));
        });
    }
}
