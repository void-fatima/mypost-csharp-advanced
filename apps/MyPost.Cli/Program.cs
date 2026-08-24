using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyPost.Application.Shipments;
using MyPost.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMyPostInfrastructure(builder.Configuration);
using var host = builder.Build();

if (args.Length != 2 || !string.Equals(args[0], "track", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("MyPost CLI — adapter over the shared application layer");
    Console.WriteLine("Usage: dotnet run --project apps/MyPost.Cli -- track <tracking-code>");
    return;
}

using var scope = host.Services.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<ShipmentService>();
try
{
    var shipment = await service.TrackPublicAsync(args[1]);
    Console.WriteLine($"{shipment.TrackingCode} — {shipment.Status}");
    Console.WriteLine($"Destination: {shipment.Destination}");
    foreach (var item in shipment.History)
        Console.WriteLine($"{item.OccurredAtUtc:u}  {item.Status,-20} {item.Description}");
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}
