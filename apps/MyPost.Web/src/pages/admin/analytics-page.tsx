import { useQuery } from '@tanstack/react-query';
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { ErrorState, LoadingState } from '../../components/page-state';
import { statusLabels } from '../../components/status-badge';
import { Card, PageHeader } from '../../components/ui';
import { api } from '../../lib/api';
import type { ShipmentStatus } from '../../types';

export default function AnalyticsPage() {
  const query = useQuery({ queryKey: ['admin-status-analytics'], queryFn: () => api.get<Record<ShipmentStatus, number>>('/admin/analytics/statuses') });
  if (query.isLoading) return <LoadingState label="Loading operational analytics" />;
  if (query.isError) return <ErrorState error={query.error} onRetry={() => void query.refetch()} />;
  const data = Object.entries(query.data!).map(([status, count]) => ({ status: statusLabels[status as ShipmentStatus], count }));
  return <><PageHeader eyebrow="Operational analytics" title="Shipment status distribution" description="A persisted count by lifecycle state, paired with a textual equivalent for accessibility." /><Card className="p-4 sm:p-6"><div className="h-80" aria-hidden="true"><ResponsiveContainer width="100%" height="100%"><BarChart data={data} margin={{ top: 8, right: 8, left: -20, bottom: 70 }}><CartesianGrid stroke="var(--border)" vertical={false} /><XAxis dataKey="status" stroke="var(--text-muted)" angle={-35} textAnchor="end" interval={0} height={80} fontSize={11} /><YAxis allowDecimals={false} stroke="var(--text-muted)" fontSize={11} /><Tooltip contentStyle={{ background: 'var(--surface)', borderColor: 'var(--border)', borderRadius: 6 }} /><Bar dataKey="count" fill="var(--primary)" radius={[4, 4, 0, 0]} /></BarChart></ResponsiveContainer></div><table className="mt-6 w-full text-left text-sm"><caption className="mb-3 text-left font-bold">Status totals</caption><thead className="text-xs uppercase tracking-wider text-muted"><tr><th className="py-2">Status</th><th className="py-2 text-right">Shipments</th></tr></thead><tbody>{data.map((item) => <tr key={item.status} className="border-t border-line"><td className="py-3">{item.status}</td><td className="py-3 text-right font-bold">{item.count}</td></tr>)}</tbody></table></Card></>;
}
