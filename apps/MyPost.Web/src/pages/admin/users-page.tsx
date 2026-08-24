import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { SearchField } from '../../components/app-shell';
import { EmptyState, ErrorState, LoadingState } from '../../components/page-state';
import { Card, PageHeader } from '../../components/ui';
import { api } from '../../lib/api';
import type { PagedResult, UserSummary } from '../../types';

export default function UsersPage() {
  const [params, setParams] = useSearchParams();
  const search = params.get('search') ?? '';
  const query = useQuery({ queryKey: ['admin-users', search], queryFn: () => api.get<PagedResult<UserSummary>>(`/admin/users?page=1&pageSize=100&search=${encodeURIComponent(search)}`) });
  return <><PageHeader eyebrow="Administration" title="Users" description="Role and account visibility for this development environment." /><Card className="p-4"><SearchField value={search} placeholder="Search name or email" onChange={(value) => setParams(value ? { search: value } : {}, { replace: true })} /></Card>{query.isLoading && <LoadingState label="Loading users" />}{query.isError && <ErrorState error={query.error} onRetry={() => void query.refetch()} />}{query.data?.items.length === 0 && <EmptyState title="No users match" description="Try a different name or email." />}{query.data && query.data.items.length > 0 && <Card className="overflow-hidden"><div className="overflow-x-auto"><table className="w-full text-left text-sm"><caption className="sr-only">MyPost users</caption><thead className="bg-subtle text-xs uppercase tracking-wider text-muted"><tr><th className="px-4 py-3">Name</th><th className="px-4 py-3">Email</th><th className="px-4 py-3">Role</th><th className="px-4 py-3">Status</th></tr></thead><tbody>{query.data.items.map((user) => <tr key={user.id} className="border-t border-line"><td className="px-4 py-4 font-semibold">{user.displayName}</td><td className="px-4 py-4 text-muted">{user.email}</td><td className="px-4 py-4">{user.role}</td><td className="px-4 py-4"><span className={user.isActive ? 'text-success' : 'text-danger'}>{user.isActive ? '● Active' : '○ Disabled'}</span></td></tr>)}</tbody></table></div></Card>}</>;
}
