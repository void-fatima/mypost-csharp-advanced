import { useQuery } from '@tanstack/react-query';
import { ChevronLeft, ChevronRight, PackagePlus } from 'lucide-react';
import { Link, useSearchParams } from 'react-router-dom';
import { SearchField } from '../../components/app-shell';
import { EmptyState, ErrorState, LoadingState } from '../../components/page-state';
import { StatusBadge, statusLabels } from '../../components/status-badge';
import { Button, Card, PageHeader, Select } from '../../components/ui';
import { api } from '../../lib/api';
import type { PagedResult, ShipmentSummary } from '../../types';

export default function ShipmentsPage({ mode }: { mode: 'customer' | 'admin' }) {
  const [params, setParams] = useSearchParams();
  const page = Number(params.get('page') ?? 1);
  const status = params.get('status') ?? '';
  const search = params.get('search') ?? '';
  const endpoint = mode === 'admin' ? '/admin/shipments' : '/customer/shipments';
  const queryString = new URLSearchParams({ page: String(page), pageSize: '10' });
  if (status) queryString.set('status', status);
  if (search) queryString.set('search', search);
  const query = useQuery({ queryKey: ['shipments', mode, page, status, search], queryFn: () => api.get<PagedResult<ShipmentSummary>>(`${endpoint}?${queryString}`) });
  function update(key: string, value: string) { const next = new URLSearchParams(params); if (value) next.set(key, value); else next.delete(key); if (key !== 'page') next.set('page', '1'); setParams(next, { replace: true }); }
  return <>
    <PageHeader eyebrow={mode === 'admin' ? 'Operations' : 'Your account'} title={mode === 'admin' ? 'Shipment management' : 'Your shipments'} description={mode === 'admin' ? 'Search, filter, inspect, and move shipments through valid operational states.' : 'Every shipment and its complete recorded journey.'} action={mode === 'customer' ? <Link to="/customer/create-shipment"><Button><PackagePlus className="size-4" />Create shipment</Button></Link> : undefined} />
    <Card className="p-4"><div className="grid gap-3 sm:grid-cols-[1fr_220px]"><SearchField value={search} onChange={(value) => update('search', value)} /><label><span className="sr-only">Filter by status</span><Select value={status} onChange={(event) => update('status', event.target.value)}><option value="">All statuses</option>{Object.entries(statusLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}</Select></label></div></Card>
    {query.isLoading && <LoadingState label="Loading shipments" />}
    {query.isError && <ErrorState error={query.error} onRetry={() => void query.refetch()} />}
    {query.data?.items.length === 0 && <EmptyState title="No shipments match" description={search || status ? 'Clear or adjust the filters to see other shipments.' : 'Create your first shipment to begin tracking its journey.'} action={mode === 'customer' ? <Link to="/customer/create-shipment"><Button>Create shipment</Button></Link> : undefined} />}
    {query.data && query.data.items.length > 0 && <><Card className="overflow-hidden"><div className="hidden overflow-x-auto md:block"><table className="w-full border-collapse text-left text-sm"><caption className="sr-only">Shipments</caption><thead className="bg-subtle text-xs uppercase tracking-wider text-muted"><tr><Header>Tracking</Header><Header>Recipient</Header><Header>Destination</Header><Header>Status</Header><Header>Created</Header><Header><span className="sr-only">Open</span></Header></tr></thead><tbody>{query.data.items.map((shipment) => <tr key={shipment.id} className="border-t border-line"><Cell><span className="font-mono text-xs font-bold tracking-wider">{shipment.trackingCode}</span></Cell><Cell>{shipment.recipientName}</Cell><Cell>{shipment.destinationCity}</Cell><Cell><StatusBadge status={shipment.status} /></Cell><Cell>{new Date(shipment.createdAtUtc).toLocaleDateString()}</Cell><Cell><Link className="font-semibold text-brand hover:underline" to={`${shipment.id}`}>View</Link></Cell></tr>)}</tbody></table></div><div className="divide-y divide-line md:hidden">{query.data.items.map((shipment) => <Link key={shipment.id} to={`${shipment.id}`} className="block p-4 hover:bg-subtle"><div className="flex items-start justify-between gap-3"><span className="font-mono text-xs font-bold">{shipment.trackingCode}</span><StatusBadge status={shipment.status} /></div><p className="mt-3 font-semibold">{shipment.recipientName}</p><p className="mt-1 text-sm text-muted">{shipment.destinationCity} · {new Date(shipment.createdAtUtc).toLocaleDateString()}</p></Link>)}</div></Card><div className="flex items-center justify-between"><p className="text-sm text-muted">Page {query.data.page} of {Math.max(1, query.data.totalPages)} · {query.data.totalCount} shipments</p><div className="flex gap-2"><Button variant="secondary" aria-label="Previous page" disabled={page <= 1} onClick={() => update('page', String(page - 1))}><ChevronLeft className="size-4" /></Button><Button variant="secondary" aria-label="Next page" disabled={page >= query.data.totalPages} onClick={() => update('page', String(page + 1))}><ChevronRight className="size-4" /></Button></div></div></>}
  </>;
}
function Header({ children }: React.PropsWithChildren) { return <th scope="col" className="px-4 py-3 font-bold">{children}</th>; }
function Cell({ children }: React.PropsWithChildren) { return <td className="px-4 py-4">{children}</td>; }
