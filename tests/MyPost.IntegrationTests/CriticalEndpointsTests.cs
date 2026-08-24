using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MyPost.Domain.Shipments;
using MyPost.Infrastructure.Persistence;

namespace MyPost.IntegrationTests;

[TestClass]
public sealed class CriticalEndpointsTests
{
    private MyPostApiFactory _factory = null!;

    [TestInitialize]
    public void Initialize() => _factory = new MyPostApiFactory();

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task AdminEndpoint_RejectsAnonymousRequests()
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/admin/shipments");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task PublicTracking_ReturnsSafeProjection()
    {
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MyPostDbContext>();
            dbContext.Shipments.Add(new Shipment(
                Guid.NewGuid(),
                "MP-TEST-100001",
                new AddressSnapshot("Home", "Secret sender street", "Tehran", "Tehran", "1111111111"),
                "Jane Smith",
                "09123456789",
                new AddressSnapshot("Office", "Secret destination street", "Shiraz", "Fars", "2222222222"),
                ShipmentType.Letter,
                120,
                null,
                ServiceLevel.Standard,
                80_000,
                DateTimeOffset.UtcNow));
            await dbContext.SaveChangesAsync();
        }

        var response = await _factory.CreateClient().GetAsync("/api/v1/tracking/MP-TEST-100001");
        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(body, "J. S.");
        Assert.IsFalse(body.Contains("09123456789", StringComparison.Ordinal));
        Assert.IsFalse(body.Contains("Secret sender street", StringComparison.Ordinal));
        Assert.IsFalse(body.Contains("Secret destination street", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task UnknownTrackingCode_ReturnsProblemDetailsWithoutExceptionLeak()
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/tracking/MP-UNKNOWN");
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.IsNotNull(problem);
        Assert.IsTrue(problem.ContainsKey("traceId"));
        Assert.IsFalse(problem.ContainsKey("stackTrace"));
    }
}
