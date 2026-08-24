using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyPost.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MyPostDbContext>
{
    public MyPostDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MyPost")
            ?? "Host=localhost;Database=mypost;Username=mypost";
        var options = new DbContextOptionsBuilder<MyPostDbContext>().UseNpgsql(connectionString).Options;
        return new MyPostDbContext(options);
    }
}
