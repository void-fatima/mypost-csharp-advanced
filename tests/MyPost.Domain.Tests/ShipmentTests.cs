using MyPost.Domain.Common;
using MyPost.Domain.Shipments;

namespace MyPost.Domain.Tests;

[TestClass]
public sealed class ShipmentTests
{
    private static readonly Guid SenderId = Guid.NewGuid();
    private static readonly Guid CourierId = Guid.NewGuid();
    private static readonly Guid AdminId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void Constructor_ValidatesParcelDimensionsAndWeight()
    {
        Assert.ThrowsException<DomainException>(() => Create(type: ShipmentType.Parcel, dimensions: null));
        Assert.ThrowsException<DomainException>(() => Create(weight: 0));
    }

    [TestMethod]
    public void TransitionTo_RejectsIllegalAndTerminalTransitions()
    {
        var shipment = Create();

        Assert.ThrowsException<DomainException>(() => shipment.TransitionTo(ShipmentStatus.Delivered, Now, AdminId, "Invalid"));
        shipment.TransitionTo(ShipmentStatus.AwaitingPickup, Now, AdminId, "Ready");
        shipment.TransitionTo(ShipmentStatus.Cancelled, Now, SenderId, "Cancelled");

        Assert.ThrowsException<DomainException>(() => shipment.TransitionTo(ShipmentStatus.Created, Now, AdminId, "Invalid"));
    }

    [TestMethod]
    public void RepeatedTransition_DoesNotDuplicateTrackingEvents()
    {
        var shipment = Create();
        shipment.TransitionTo(ShipmentStatus.AwaitingPickup, Now, AdminId, "Ready");

        var changed = shipment.TransitionTo(ShipmentStatus.AwaitingPickup, Now, AdminId, "Duplicate");

        Assert.IsFalse(changed);
        Assert.AreEqual(2, shipment.TrackingEvents.Count);
    }

    [TestMethod]
    public void AssignedCourier_CanRecordFailureAndShipmentCanReturn()
    {
        var shipment = Create();
        shipment.TransitionTo(ShipmentStatus.AwaitingPickup, Now, AdminId, "Ready");
        shipment.TransitionTo(ShipmentStatus.Accepted, Now, AdminId, "Accepted");
        shipment.TransitionTo(ShipmentStatus.InTransit, Now, AdminId, "In transit");
        shipment.TransitionTo(ShipmentStatus.OutForDelivery, Now, AdminId, "Out for delivery");
        shipment.AssignCourier(CourierId, AdminId, Now);

        shipment.RecordDelivery(DeliveryResult.RecipientUnavailable, "No answer", Now, CourierId);
        shipment.InitiateReturn(Now, AdminId, "Attempts exhausted");
        shipment.TransitionTo(ShipmentStatus.ReturningToSender, Now, AdminId, "Returning");
        shipment.TransitionTo(ShipmentStatus.ReturnedToSender, Now, AdminId, "Returned");

        Assert.AreEqual(ShipmentStatus.ReturnedToSender, shipment.Status);
    }

    [TestMethod]
    public void ReassigningCourier_EndsPreviousAssignmentAndIsDuplicateSafe()
    {
        var shipment = Create();
        var nextCourier = Guid.NewGuid();

        Assert.IsTrue(shipment.AssignCourier(CourierId, AdminId, Now));
        Assert.IsFalse(shipment.AssignCourier(CourierId, AdminId, Now));
        Assert.IsTrue(shipment.AssignCourier(nextCourier, AdminId, Now.AddMinutes(1)));

        Assert.AreEqual(2, shipment.Assignments.Count);
        Assert.IsFalse(shipment.Assignments.First().IsActive);
        Assert.IsTrue(shipment.Assignments.Last().IsActive);
    }

    private static Shipment Create(
        ShipmentType type = ShipmentType.Letter,
        decimal weight = 100,
        Dimensions? dimensions = null) =>
        new(
            SenderId,
            "MP-260101-ABC123",
            new AddressSnapshot("Home", "1 Sender St", "Tehran", "Tehran", "1111111111"),
            "Jane Smith",
            "09123456789",
            new AddressSnapshot("Office", "2 Destination St", "Shiraz", "Fars", "2222222222"),
            type,
            weight,
            dimensions,
            ServiceLevel.Standard,
            120_000m,
            Now);
}
