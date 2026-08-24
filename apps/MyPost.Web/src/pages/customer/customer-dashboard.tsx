import { useQuery } from '@tanstack/react-query';
import { ArrowRight, Boxes, CircleCheck, Clock3, PackagePlus, Route } from 'lucide-react';
import { Link } from 'react-router-dom';
import { EmptyState, ErrorState, LoadingState } from '../../components/page-state';
import { StatusBadge } from '../../components/status-badge';
import { Button, Card, PageHeader } from '../../components/ui';
import { api } from '../../lib/api';
import type { PagedResult, ShipmentSummary } from '../../types';

export default function CustomerDashboard() {
  const query = useQuery({ queryKey: ['shipments', 'customer', 'dashboard'], queryFn: () => api.get<PagedResult<ShipmentSummary>>('/customer/shipments?page=1&pageSize=20') });
  if (query.isLoading) return <LoadingState label="Loading customer dashboard" />;
  if (query.isError) return <ErrorState error={query.error} onRetry={() => void query.refetch()} />;
  const shipments = query.data!.items;
  const active = shipments.filter((item) => !['Delivered', 'ReturnedToSender', 'Cancelled'].includes(item.status)).length;
  const delivered = shipments.filter((item) => item.status === 'Delivered').length;
  return <><PageHeader eyebrow="Customer workspace" title="Your postal overview" description="Create, follow, and understand every virtual shipment from one place." action={<Link to="/customer/create-shipment"><Button><PackagePlus className="size-4" />Create shipment</Button></Link>} /><div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4"><Metric icon={Boxes} label="Total shipments" value={query.data!.totalCount} /><Metric icon={Route} label="Active journeys" value={active} /><Metric icon={CircleCheck} label="Delivered" value={delivered} /><Metric icon={Clock3} label="Needs attention" value={shipments.filter((item) => item.status === 'DeliveryFailed').length} /></div><div><div className="mb-4 flex items-end justify-between"><div><h2 className="text-xl font-bold">Recent shipments</h2><p className="mt-1 text-sm text-muted">Latest recorded activity</p></div><Link className="inline-flex min-h-11 items-center gap-1 text-sm font-semibold text-brand" to="/customer/shipments">View all<ArrowRight className="size-4" /></Link></div>{shipments.length === 0 ? <EmptyState title="No shipments yet" description="Add a sender address, then create your first virtual shipment." action={<Link to="/customer/addresses"><Button variant="secondary">Add an address</Button></Link>} /> : <Card className="divide-y divide-line overflow-hidden">{shipments.slice(0, 5).map((shipment) => <Link key={shipment.id} className="grid gap-3 p-4 hover:bg-subtle sm:grid-cols-[1fr_auto_auto] sm:items-center" to={`/customer/shipments/${shipment.id}`}><div><p className="font-mono text-xs font-bold tracking-wider">{shipment.trackingCode}</p><p className="mt-1 font-semibold">{shipment.recipientName} · {shipment.destinationCity}</p></div><StatusBadge status={shipment.status} /><span className="text-xs text-muted">{new Date(shipment.createdAtUtc).toLocaleDateString()}</span></Link>)}</Card>}</div></>;
}
function Metric({ icon: Icon, label, value }: { icon: typeof Boxes; label: string; value: number }) { return <Card className="p-5"><div className="flex items-center justify-between"><span className="text-sm font-semibold text-muted">{label}</span><Icon className="size-5 text-brand" /></div><p className="mt-4 text-3xl font-bold tabular-nums">{value}</p></Card>; }
