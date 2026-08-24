# Shipment lifecycle

The aggregate is the only place allowed to mutate shipment status. Legal transitions are:

`Created → AwaitingPickup → Accepted → InTransit → OutForDelivery → Delivered`

Cancellation is allowed only from `Created` or `AwaitingPickup`. A failed delivery moves to `DeliveryFailed`, where another delivery attempt may begin or an administrator may initiate `ReturnInitiated → ReturningToSender → ReturnedToSender`. Accepted or in-transit shipments may also enter the return flow when operations requires it.

`Delivered`, `ReturnedToSender`, and `Cancelled` are terminal. Repeating the current status or assigning the same active courier is an idempotent no-op and does not append duplicate history or assignment records.
