import { useQuery } from '@tanstack/react-query';
import { MapPin, Route, Truck } from 'lucide-react';
import { Link } from 'react-router-dom';
import { EmptyState, ErrorState, LoadingState } from '../../components/page-state';
import { StatusBadge } from '../../components/status-badge';
import { Card, PageHeader } from '../../components/ui';
import { api } from '../../lib/api';
import type { PagedResult, ShipmentSummary } from '../../types';

export default function CourierDashboard() {
  const query = useQuery({ queryKey: ['courier-shipments'], queryFn: () => api.get<PagedResult<ShipmentSummary>>('/courier/shipments?page=1&pageSize=50') });
  if (query.isLoading) return <LoadingState label="Loading assigned deliveries" />;
  if (query.isError) return <ErrorState error={query.error} onRetry={() => void query.refetch()} />;
  const items = query.data!.items;
  return <><PageHeader eyebrow="Courier workspace" title="Assigned deliveries" description="Only shipments currently assigned to your account appear here. Open one to record the next legal milestone." /><div className="grid gap-4 sm:grid-cols-3"><Metric icon={Truck} label="Assigned" value={items.length} /><Metric icon={Route} label="Out for delivery" value={items.filter((item) => item.status === 'OutForDelivery').length} /><Metric icon={MapPin} label="Needs attention" value={items.filter((item) => item.status === 'DeliveryFailed').length} /></div>{items.length === 0 ? <EmptyState title="No assigned deliveries" description="An administrator has not assigned active shipments to you." /> : <div className="grid gap-4 lg:grid-cols-2">{items.map((shipment) => <Link key={shipment.id} to={`/courier/shipments/${shipment.id}`}><Card className="h-full p-5 transition-colors hover:border-brand"><div className="flex items-start justify-between gap-3"><span className="font-mono text-xs font-bold tracking-wider">{shipment.trackingCode}</span><StatusBadge status={shipment.status} /></div><h2 className="mt-5 text-lg font-bold">{shipment.recipientName}</h2><p className="mt-1 text-sm text-muted">{shipment.destinationCity} · {shipment.serviceLevel} {shipment.type.toLowerCase()}</p></Card></Link>)}</div>}</>;
}
function Metric({ icon: Icon, label, value }: { icon: typeof Truck; label: string; value: number }) { return <Card className="p-5"><Icon className="size-5 text-brand" /><p className="mt-4 text-3xl font-bold">{value}</p><p className="mt-1 text-sm text-muted">{label}</p></Card>; }
