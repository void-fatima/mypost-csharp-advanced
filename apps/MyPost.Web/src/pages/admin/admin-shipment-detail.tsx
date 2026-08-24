import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { RotateCcw, UserRoundCheck } from 'lucide-react';
import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { ErrorState, LoadingState } from '../../components/page-state';
import { StatusBadge, statusLabels } from '../../components/status-badge';
import { TrackingTimeline } from '../../components/tracking-timeline';
import { Button, Card, Field, Input, PageHeader, Select } from '../../components/ui';
import { api } from '../../lib/api';
import type { PagedResult, ShipmentDetail, ShipmentStatus, UserSummary } from '../../types';

export default function AdminShipmentDetail() {
  const { id } = useParams();
  const client = useQueryClient();
  const [courierId, setCourierId] = useState('');
  const [status, setStatus] = useState<ShipmentStatus>('Accepted');
  const [description, setDescription] = useState('');
  const [returnReason, setReturnReason] = useState('');
  const [message, setMessage] = useState('');
  const shipment = useQuery({ queryKey: ['shipment', 'admin', id], queryFn: () => api.get<ShipmentDetail>(`/admin/shipments/${id}`) });
  const users = useQuery({ queryKey: ['admin-users', 'couriers'], queryFn: () => api.get<PagedResult<UserSummary>>('/admin/users?page=1&pageSize=100&search=') });
  async function refresh(messageText: string) { setMessage(messageText); await client.invalidateQueries({ queryKey: ['shipment', 'admin', id] }); await client.invalidateQueries({ queryKey: ['admin-overview'] }); }
  const assign = useMutation({ mutationFn: () => api.post<void>(`/admin/shipments/${id}/assign`, { courierUserId: courierId }), onSuccess: () => refresh('Courier assignment saved.') });
  const transition = useMutation({ mutationFn: () => api.post<void>(`/admin/shipments/${id}/status`, { status, description }), onSuccess: () => refresh('Shipment status updated.') });
  const initiateReturn = useMutation({ mutationFn: () => api.post<void>(`/admin/shipments/${id}/return`, { reason: returnReason }), onSuccess: () => refresh('Return-to-sender initiated.') });
  if (shipment.isLoading || users.isLoading) return <LoadingState label="Loading operational shipment detail" />;
  if (shipment.isError) return <ErrorState error={shipment.error} onRetry={() => void shipment.refetch()} />;
  if (users.isError) return <ErrorState error={users.error} onRetry={() => void users.refetch()} />;
  const data = shipment.data!;
  const couriers = users.data!.items.filter((user) => user.role === 'Courier' && user.isActive);
  const error = assign.error ?? transition.error ?? initiateReturn.error;
  return <><PageHeader eyebrow="Operational shipment" title={data.trackingCode} description={`${data.recipientName} · ${data.destinationAddress.city}`} action={<StatusBadge status={data.status} />} />{message && <p className="rounded-control bg-success/10 p-3 text-sm text-success" role="status">{message}</p>}{error && <p className="rounded-control bg-danger/10 p-3 text-sm text-danger" role="alert">{error.message}</p>}<div className="grid gap-6 xl:grid-cols-[.8fr_1.2fr]"><div className="space-y-6"><Card className="p-6"><h2 className="font-bold">Courier assignment</h2><div className="mt-4 space-y-4"><Field label="Active courier" htmlFor="courier"><Select id="courier" value={courierId} onChange={(event) => setCourierId(event.target.value)}><option value="">Select courier</option>{couriers.map((user) => <option key={user.id} value={user.id}>{user.displayName}</option>)}</Select></Field><Button disabled={!courierId || assign.isPending} onClick={() => assign.mutate()}><UserRoundCheck className="size-4" />{assign.isPending ? 'Assigning…' : 'Assign courier'}</Button></div></Card><Card className="p-6"><h2 className="font-bold">Status transition</h2><div className="mt-4 space-y-4"><Field label="Next status" htmlFor="next-status"><Select id="next-status" value={status} onChange={(event) => setStatus(event.target.value as ShipmentStatus)}>{Object.entries(statusLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}</Select></Field><Field label="Public-facing description" htmlFor="description"><Input id="description" value={description} onChange={(event) => setDescription(event.target.value)} /></Field><Button disabled={!description || transition.isPending} onClick={() => transition.mutate()}>{transition.isPending ? 'Updating…' : 'Update status'}</Button></div></Card><Card className="border-warning/40 p-6"><h2 className="font-bold">Return to sender</h2><p className="mt-2 text-sm text-muted">Available only from domain-approved states. The server rejects illegal transitions.</p><div className="mt-4 space-y-4"><Field label="Reason" htmlFor="return-reason"><Input id="return-reason" value={returnReason} onChange={(event) => setReturnReason(event.target.value)} /></Field><Button variant="secondary" disabled={!returnReason || initiateReturn.isPending} onClick={() => { if (window.confirm('Initiate return-to-sender?')) initiateReturn.mutate(); }}><RotateCcw className="size-4" />Initiate return</Button></div></Card></div><Card className="p-6"><h2 className="mb-6 text-xl font-bold">Persisted tracking history</h2><TrackingTimeline events={data.history} /></Card></div></>;
}
