import { useQuery } from '@tanstack/react-query';
import { Search } from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { Logo } from '../../components/logo';
import { ErrorState, LoadingState } from '../../components/page-state';
import { StatusBadge } from '../../components/status-badge';
import { TrackingTimeline } from '../../components/tracking-timeline';
import { Button, Card } from '../../components/ui';
import { api } from '../../lib/api';
import type { PublicTracking } from '../../types';

export default function TrackingPage() {
  const { trackingCode } = useParams();
  const [code, setCode] = useState(trackingCode ?? '');
  const navigate = useNavigate();
  const query = useQuery({ queryKey: ['public-tracking', trackingCode], queryFn: () => api.get<PublicTracking>(`/tracking/${encodeURIComponent(trackingCode!)}`), enabled: Boolean(trackingCode) });
  function submit(event: React.FormEvent) { event.preventDefault(); if (code.trim()) navigate(`/track/${encodeURIComponent(code.trim().toUpperCase())}`); }
  return (
    <div className="min-h-screen bg-canvas"><header className="border-b border-line bg-surface"><div className="mx-auto flex h-18 max-w-5xl items-center justify-between px-4 sm:px-6"><Logo /><Link className="text-sm font-semibold text-brand" to="/login">Sign in</Link></div></header><main className="mx-auto max-w-5xl space-y-8 px-4 py-10 sm:px-6">
      <div className="max-w-2xl"><p className="text-xs font-bold uppercase tracking-[0.16em] text-brand">Public tracking</p><h1 className="mt-3 text-4xl font-bold tracking-tight">Follow a shipment</h1><p className="mt-3 leading-7 text-muted">This privacy-safe view shows operational milestones without exposing sender details, phone numbers, or street addresses.</p></div>
      <form onSubmit={submit} className="flex flex-col gap-3 sm:flex-row"><label className="sr-only" htmlFor="tracking-code">Tracking code</label><input id="tracking-code" className="min-h-12 flex-1 rounded-control border border-line bg-surface px-4 font-mono uppercase tracking-wider" value={code} onChange={(event) => setCode(event.target.value)} placeholder="MP-DEMO-100004" /><Button type="submit" className="min-h-12"><Search className="size-4" />Track</Button></form>
      {!trackingCode && <Card className="p-8 text-center"><h2 className="font-bold">Enter a tracking code to begin</h2><p className="mt-2 text-sm text-muted">Tracking is available without an account.</p></Card>}
      {query.isLoading && <LoadingState label="Looking up shipment" />}
      {query.isError && <ErrorState error={query.error} onRetry={() => void query.refetch()} />}
      {query.data && <div className="grid gap-6 lg:grid-cols-[.8fr_1.2fr]"><Card className="h-fit p-6"><p className="text-xs font-bold uppercase tracking-wider text-muted">Tracking code</p><p className="mt-2 font-mono text-sm font-bold tracking-wider">{query.data.trackingCode}</p><div className="mt-6"><StatusBadge status={query.data.status} /></div><dl className="mt-6 grid gap-4 border-t border-line pt-5 text-sm"><Fact label="Recipient" value={query.data.recipient} /><Fact label="Destination" value={query.data.destination} /><Fact label="Service" value={`${query.data.serviceLevel} ${query.data.type.toLowerCase()}`} /><Fact label="Created" value={new Date(query.data.createdAtUtc).toLocaleDateString()} /></dl></Card><Card className="p-6"><h2 className="mb-6 text-lg font-bold">Shipment timeline</h2><TrackingTimeline events={query.data.history} /></Card></div>}
    </main></div>
  );
}
function Fact({ label, value }: { label: string; value: string }) { return <div className="flex justify-between gap-4"><dt className="text-muted">{label}</dt><dd className="text-right font-semibold">{value}</dd></div>; }
