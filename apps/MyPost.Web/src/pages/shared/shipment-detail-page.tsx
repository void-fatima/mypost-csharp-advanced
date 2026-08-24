import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Ban, Copy } from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ErrorState, LoadingState } from '../../components/page-state';
import { StatusBadge } from '../../components/status-badge';
import { TrackingTimeline } from '../../components/tracking-timeline';
import { Button, Card, PageHeader } from '../../components/ui';
import { api } from '../../lib/api';
import type { ShipmentDetail } from '../../types';

export default function ShipmentDetailPage({ mode }: { mode: 'customer' | 'admin' }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const client = useQueryClient();
  const [message, setMessage] = useState('');
  const prefix = mode === 'admin' ? '/admin' : '/customer';
  const query = useQuery({ queryKey: ['shipment', mode, id], queryFn: () => api.get<ShipmentDetail>(`${prefix}/shipments/${id}`) });
  const cancel = useMutation({ mutationFn: () => api.post<void>(`/customer/shipments/${id}/cancel`), onSuccess: async () => { setMessage('Shipment cancelled.'); await client.invalidateQueries({ queryKey: ['shipment', mode, id] }); } });
  if (query.isLoading) return <LoadingState label="Loading shipment details" />;
  if (query.isError) return <ErrorState error={query.error} onRetry={() => void query.refetch()} />;
  const shipment = query.data!;
  const canCancel = mode === 'customer' && ['Created', 'AwaitingPickup'].includes(shipment.status);
  return <>
    <PageHeader eyebrow="Shipment detail" title={shipment.trackingCode} description={`Created ${new Date(shipment.createdAtUtc).toLocaleString()} · ${shipment.serviceLevel} ${shipment.type.toLowerCase()}`} action={<div className="flex gap-2"><Button variant="secondary" onClick={() => { void navigator.clipboard.writeText(shipment.trackingCode); setMessage('Tracking code copied.'); }}><Copy className="size-4" />Copy code</Button>{canCancel && <Button variant="danger" disabled={cancel.isPending} onClick={() => { if (window.confirm('Cancel this shipment? This cannot be undone.')) cancel.mutate(); }}><Ban className="size-4" />{cancel.isPending ? 'Cancelling…' : 'Cancel'}</Button>}</div>} />
    {message && <p className="rounded-control border border-success/30 bg-success/10 p-3 text-sm text-success" role="status">{message}</p>}
    {cancel.isError && <p className="rounded-control bg-danger/10 p-3 text-sm text-danger" role="alert">{cancel.error.message}</p>}
    <div className="grid gap-6 xl:grid-cols-[.8fr_1.2fr]"><div className="space-y-6"><Card className="p-6"><div className="flex items-start justify-between"><div><p className="text-xs font-bold uppercase tracking-wider text-muted">Current status</p><div className="mt-3"><StatusBadge status={shipment.status} /></div></div><p className="text-right text-sm font-bold">{shipment.calculatedPrice.toLocaleString()} IRR</p></div><dl className="mt-6 grid grid-cols-2 gap-4 border-t border-line pt-5"><Fact label="Recipient" value={shipment.recipientName} /><Fact label="Phone" value={shipment.recipientPhone} /><Fact label="Weight" value={`${shipment.weightGrams.toLocaleString()} g`} /><Fact label="Courier" value={shipment.courierUserId ? 'Assigned' : 'Unassigned'} /></dl></Card><AddressCard title="Sender / return" value={shipment.senderAddress} /><AddressCard title="Destination" value={shipment.destinationAddress} /></div><Card className="p-6"><h2 className="mb-6 text-xl font-bold">Tracking history</h2><TrackingTimeline events={shipment.history} />{mode === 'customer' && <Link className="mt-2 inline-block text-sm font-semibold text-brand hover:underline" to={`/track/${shipment.trackingCode}`}>Open public tracking view</Link>}</Card></div>
    <Button variant="ghost" onClick={() => navigate(-1)}>← Back to shipments</Button>
  </>;
}
function Fact({ label, value }: { label: string; value: string }) { return <div><dt className="text-xs font-bold uppercase tracking-wider text-muted">{label}</dt><dd className="mt-1 text-sm font-semibold">{value}</dd></div>; }
function AddressCard({ title, value }: { title: string; value: ShipmentDetail['senderAddress'] }) { return <Card className="p-6"><h2 className="font-bold">{title}</h2><address className="mt-3 text-sm not-italic leading-6 text-muted"><strong className="text-ink">{value.label}</strong><br />{value.line1}<br />{value.city}, {value.province}<br />{value.postalCode} · {value.country}</address></Card>; }
