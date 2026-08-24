using MyPost.Application.Abstractions;
using MyPost.Application.Addresses;
using MyPost.Application.Common;
using MyPost.Application.Shipments;
using MyPost.Application.Users;
using MyPost.Domain.Addresses;
using MyPost.Domain.Shipments;
using MyPost.Domain.Users;

namespace MyPost.Application.Tests;

[TestClass]
public sealed class ShipmentServiceTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public async Task CreateAsync_IsIdempotentForCustomerReference()
    {
        var fixture = new Fixture(CustomerId);
        var request = fixture.Request("checkout-42");

        var first = await fixture.Service.CreateAsync(CustomerId, request);
        var second = await fixture.Service.CreateAsync(CustomerId, request);

        Assert.AreEqual(first.Id, second.Id);
        Assert.AreEqual(1, fixture.Shipments.AddCount);
        Assert.AreEqual(1, fixture.UnitOfWork.SaveCount);
    }

    [TestMethod]
    public async Task CreateAsync_RejectsAddressOwnedByAnotherCustomer()
    {
        var fixture = new Fixture(Guid.NewGuid());

        await Assert.ThrowsExceptionAsync<ForbiddenException>(() => fixture.Service.CreateAsync(CustomerId, fixture.Request(null)));
    }

    [TestMethod]
    public async Task PublicTracking_ReturnsMaskedRecipientAndNoPrivateAddressLine()
    {
        var fixture = new Fixture(CustomerId);
        var created = await fixture.Service.CreateAsync(CustomerId, fixture.Request(null));

        var tracked = await fixture.Service.TrackPublicAsync(created.TrackingCode);

        Assert.AreEqual("J. S.", tracked.Recipient);
        Assert.AreEqual("Shiraz, Fars", tracked.Destination);
        Assert.IsFalse(tracked.Destination.Contains("Secret destination", StringComparison.Ordinal));
    }

    private sealed class Fixture
    {
        private readonly Address _address;

        public Fixture(Guid addressOwner)
        {
            _address = new Address(addressOwner, "Home", "Private sender address", "Tehran", "Tehran", "1111111111", true);
            Shipments = new FakeShipmentRepository();
            UnitOfWork = new FakeUnitOfWork();
            Service = new ShipmentService(
                Shipments,
                new FakeAddressRepository(_address),
                new FakeUsers(),
                new FakeTrackingCodes(),
                new ShipmentPriceCalculator(),
                new FakeClock(),
                UnitOfWork);
        }

        public FakeShipmentRepository Shipments { get; }
        public FakeUnitOfWork UnitOfWork { get; }
        public ShipmentService Service { get; }

        public CreateShipmentRequest Request(string? reference) =>
            new(
                _address.Id,
                "Jane Smith",
                "09123456789",
                new DestinationAddressRequest("Office", "Secret destination", "Shiraz", "Fars", "2222222222"),
                ShipmentType.Letter,
                150,
                null,
                ServiceLevel.Standard,
                reference);
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow => Now;
    }

    private sealed class FakeTrackingCodes : ITrackingCodeGenerator
    {
        private int _value;
        public string Create(DateTimeOffset nowUtc) => $"MP-260101-{++_value:000000}";
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAddressRepository(Address address) : IAddressRepository
    {
        public Task<Address?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Address?>(id == address.Id ? address : null);
        public Task<IReadOnlyList<Address>> ListOwnedAsync(Guid ownerUserId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Address>>([]);
        public Task AddAsync(Address value, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(Address value) { }
    }

    private sealed class FakeShipmentRepository : IShipmentRepository
    {
        private readonly List<Shipment> _items = [];
        public int AddCount { get; private set; }
        public Task<Shipment?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_items.SingleOrDefault(item => item.Id == id));
        public Task<Shipment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken = default) => Task.FromResult(_items.SingleOrDefault(item => item.TrackingCode == trackingCode));
        public Task<Shipment?> GetByCustomerReferenceAsync(Guid senderUserId, string customerReference, CancellationToken cancellationToken = default) => Task.FromResult(_items.SingleOrDefault(item => item.SenderUserId == senderUserId && item.CustomerReference == customerReference));
        public Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken cancellationToken = default) => Task.FromResult(_items.Any(item => item.TrackingCode == trackingCode));
        public Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
        {
            AddCount++;
            _items.Add(shipment);
            return Task.CompletedTask;
        }
        public Task<PagedResult<Shipment>> SearchAsync(ShipmentFilter filter, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<Shipment>(_items, 1, 20, _items.Count));
    }

    private sealed class FakeUsers : IUserDirectory
    {
        public Task<bool> IsInRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default) => Task.FromResult(role == UserRole.Courier);
        public Task<PagedResult<UserSummaryDto>> ListAsync(PageRequest page, string? search, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<UserSummaryDto>([], 1, 20, 0));
    }
}
