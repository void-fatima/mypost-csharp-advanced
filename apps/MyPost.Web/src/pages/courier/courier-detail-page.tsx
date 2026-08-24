import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CheckCircle2, Route, Truck } from 'lucide-react';
import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { ErrorState, LoadingState } from '../../components/page-state';
import { StatusBadge } from '../../components/status-badge';
import { TrackingTimeline } from '../../components/tracking-timeline';
import { Button, Card, PageHeader, Select, Textarea } from '../../components/ui';
import { api } from '../../lib/api';
import type { DeliveryResult, ShipmentDetail } from '../../types';

export default function CourierDetailPage() {
  const { id } = useParams();
  const client = useQueryClient();
  const [result, setResult] = useState<DeliveryResult>('Delivered');
  const [note, setNote] = useState('');
  const [message, setMessage] = useState('');
  const query = useQuery({ queryKey: ['courier-shipment', id], queryFn: () => api.get<ShipmentDetail>(`/courier/shipments/${id}`) });
  const update = useMutation({ mutationFn: (body: unknown) => api.post<void>(`/courier/shipments/${id}/status`, body), onSuccess: refresh });
  const delivery = useMutation({ mutationFn: () => api.post<void>(`/courier/shipments/${id}/delivery`, { result, note }), onSuccess: refresh });
  async function refresh() { setMessage('Shipment updated successfully.'); await client.invalidateQueries({ queryKey: ['courier-shipment', id] }); }
  if (query.isLoading) return <LoadingState label="Loading delivery assignment" />;
  if (query.isError) return <ErrorState error={query.error} onRetry={() => void query.refetch()} />;
  const shipment = query.data!;
  return <><PageHeader eyebrow="Delivery assignment" title={shipment.trackingCode} description={`${shipment.recipientName} · ${shipment.destinationAddress.city}`} action={<StatusBadge status={shipment.status} />} />{message && <p className="rounded-control bg-success/10 p-3 text-sm text-success" role="status">{message}</p>}<div className="grid gap-6 xl:grid-cols-[.8fr_1.2fr]"><div className="space-y-6"><Card className="p-6"><h2 className="font-bold">Destination</h2><address className="mt-3 not-italic leading-7 text-muted"><strong className="text-ink">{shipment.recipientName}</strong><br />{shipment.destinationAddress.line1}<br />{shipment.destinationAddress.city}, {shipment.destinationAddress.province}<br />{shipment.destinationAddress.postalCode}<br />{shipment.recipientPhone}</address></Card><Card className="p-6"><h2 className="font-bold">Next operational step</h2><div className="mt-4 flex flex-wrap gap-2">{shipment.status === 'Accepted' && <Button disabled={update.isPending} onClick={() => update.mutate({ status: 'InTransit', description: 'Courier collected shipment' })}><Route className="size-4" />Mark in transit</Button>}{shipment.status === 'InTransit' && <Button disabled={update.isPending} onClick={() => update.mutate({ status: 'OutForDelivery', description: 'Courier started final delivery' })}><Truck className="size-4" />Start delivery</Button>}{shipment.status === 'OutForDelivery' && <form className="w-full space-y-4" onSubmit={(event) => { event.preventDefault(); delivery.mutate(); }}><label className="block text-sm font-semibold">Delivery result<Select className="mt-2" value={result} onChange={(event) => setResult(event.target.value as DeliveryResult)}><option value="Delivered">Delivered</option><option value="RecipientUnavailable">Recipient unavailable</option><option value="AddressNotFound">Address not found</option><option value="Refused">Refused</option><option value="Damaged">Damaged</option><option value="Other">Other</option></Select></label><label className="block text-sm font-semibold">Delivery note<Textarea className="mt-2" value={note} onChange={(event) => setNote(event.target.value)} placeholder="Optional operational note" /></label><Button type="submit" disabled={delivery.isPending}><CheckCircle2 className="size-4" />{delivery.isPending ? 'Recording…' : 'Record result'}</Button></form>}{!['Accepted', 'InTransit', 'OutForDelivery'].includes(shipment.status) && <p className="text-sm leading-6 text-muted">No courier action is available in the current state.</p>}</div>{(update.isError || delivery.isError) && <p className="mt-4 text-sm text-danger" role="alert">{(update.error ?? delivery.error)?.message}</p>}</Card></div><Card className="p-6"><h2 className="mb-6 text-xl font-bold">Recorded journey</h2><TrackingTimeline events={shipment.history} /></Card></div></>;
}
