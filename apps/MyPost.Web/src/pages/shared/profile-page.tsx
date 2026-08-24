import { useAuth } from '../../auth/auth-context';
import { Card, PageHeader } from '../../components/ui';

export default function ProfilePage() {
  const { user } = useAuth();
  if (!user) return null;
  return <><PageHeader eyebrow="Account" title="Profile and settings" description="Your current identity and secure session preferences." /><Card className="max-w-2xl p-6"><dl className="grid gap-5 sm:grid-cols-2"><Fact label="Display name" value={user.displayName} /><Fact label="Email" value={user.email} /><Fact label="Role" value={user.role} /><Fact label="Session" value="Short-lived access + HttpOnly refresh" /></dl><div className="mt-7 border-t border-line pt-5"><h2 className="font-bold">Privacy note</h2><p className="mt-2 text-sm leading-6 text-muted">Authentication tokens are never stored in browser local storage. Account editing is intentionally limited in this portfolio version.</p></div></Card></>;
}
function Fact({ label, value }: { label: string; value: string }) { return <div><dt className="text-xs font-bold uppercase tracking-wider text-muted">{label}</dt><dd className="mt-1 font-semibold">{value}</dd></div>; }
