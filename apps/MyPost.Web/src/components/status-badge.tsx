import { AlertTriangle, Ban, CheckCircle2, Clock3, PackageCheck, RotateCcw, Route, Truck } from 'lucide-react';
import clsx from 'clsx';
import type { ShipmentStatus } from '../types';

export const statusLabels: Record<ShipmentStatus, string> = {
  Created: 'Created', AwaitingPickup: 'Awaiting pickup', Accepted: 'Accepted', InTransit: 'In transit', OutForDelivery: 'Out for delivery', DeliveryFailed: 'Delivery failed', Delivered: 'Delivered', ReturnInitiated: 'Return initiated', ReturningToSender: 'Returning to sender', ReturnedToSender: 'Returned to sender', Cancelled: 'Cancelled',
};

export function StatusBadge({ status }: { status: ShipmentStatus }) {
  const Icon = status === 'Delivered' ? CheckCircle2 : status === 'DeliveryFailed' ? AlertTriangle : status === 'Cancelled' ? Ban : status.includes('Return') ? RotateCcw : status === 'OutForDelivery' ? Truck : status === 'InTransit' ? Route : status === 'Accepted' ? PackageCheck : Clock3;
  const tone = status === 'Delivered' ? 'text-success bg-success/10' : status === 'DeliveryFailed' || status === 'Cancelled' ? 'text-danger bg-danger/10' : status.includes('Return') || status === 'AwaitingPickup' ? 'text-warning bg-warning/10' : 'text-info bg-info/10';
  return <span className={clsx('inline-flex w-fit items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-bold', tone)}><Icon className="size-3.5" aria-hidden="true" />{statusLabels[status]}</span>;
}
