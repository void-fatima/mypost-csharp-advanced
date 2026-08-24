import { useQuery } from '@tanstack/react-query';
import { AlertTriangle, Boxes, CircleCheck, Clock3, RotateCcw, Truck } from 'lucide-react';
import { Link } from 'react-router-dom';
import { ErrorState, LoadingState } from '../../components/page-state';
import { Button, Card, PageHeader } from '../../components/ui';
import { api } from '../../lib/api';
import type { OperationsOverview } from '../../types';

export default function AdminOverview() {
  const query = useQuery({ queryKey: ['admin-overview'], queryFn: () => api.get<OperationsOverview>('/admin/overview') });
  if (query.isLoading) return <LoadingState label="Loading operations overview" />;
  if (query.isError) return <ErrorState error={query.error} onRetry={() => void query.refetch()} />;
  const data = query.data!;
  return <><PageHeader eyebrow="Operations control" title="Network overview" description="A factual snapshot of persisted virtual shipments—no simulated live locations." action={<Link to="/admin/shipments"><Button>Manage shipments</Button></Link>} /><div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4"><Metric icon={Boxes} label="Total shipments" value={data.totalShipments} /><Metric icon={Truck} label="In active transit" value={data.inTransit + data.outForDelivery} /><Metric icon={CircleCheck} label="Delivered" value={data.delivered} /><Metric icon={AlertTriangle} label="Delivery failed" value={data.deliveryFailed} tone="danger" /></div><div className="grid gap-6 lg:grid-cols-[1.2fr_.8fr]"><Card className="p-6"><h2 className="text-xl font-bold">Operational queue</h2><div className="mt-6 grid gap-3 sm:grid-cols-2"><Queue icon={Clock3} label="Awaiting pickup" value={data.awaitingPickup} /><Queue icon={Truck} label="Out for delivery" value={data.outForDelivery} /><Queue icon={AlertTriangle} label="Failed delivery" value={data.deliveryFailed} /><Queue icon={RotateCcw} label="Return flow" value={data.returning} /></div></Card><Card className="p-6"><p className="text-xs font-bold uppercase tracking-wider text-muted">Booked shipment value</p><p className="mt-3 text-3xl font-bold tabular-nums">{data.totalRevenue.toLocaleString()} <span className="text-sm text-muted">IRR</span></p><p className="mt-4 text-sm leading-6 text-muted">Calculated virtual shipment prices excluding cancelled records. This is not settled payment revenue.</p></Card></div></>;
}
function Metric({ icon: Icon, label, value, tone }: { icon: typeof Boxes; label: string; value: number; tone?: 'danger' }) { return <Card className="p-5"><Icon className={`size-5 ${tone === 'danger' ? 'text-danger' : 'text-brand'}`} /><p className="mt-4 text-3xl font-bold">{value}</p><p className="mt-1 text-sm text-muted">{label}</p></Card>; }
function Queue({ icon: Icon, label, value }: { icon: typeof Boxes; label: string; value: number }) { return <div className="flex items-center justify-between rounded-control border border-line p-4"><span className="flex items-center gap-3 text-sm font-semibold"><Icon className="size-5 text-brand" />{label}</span><strong className="text-xl">{value}</strong></div>; }
