namespace MyPost.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
        app.Run();
    }
}
