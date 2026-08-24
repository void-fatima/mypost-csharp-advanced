import { Link } from 'react-router-dom';

export function Logo({ compact = false }: { compact?: boolean }) {
  return (
    <Link to="/" className="inline-flex min-h-11 items-center gap-2 text-ink" aria-label="MyPost home">
      <svg aria-hidden="true" viewBox="0 0 40 40" className="size-8 text-brand" fill="none">
        <path d="M7 13.5 20 6l13 7.5v14L20 35 7 27.5v-14Z" stroke="currentColor" strokeWidth="3" strokeLinejoin="round" />
        <path d="m7 13.5 13 7.2 13-7.2M20 20.7V35" stroke="currentColor" strokeWidth="3" strokeLinejoin="round" />
        <circle cx="30.5" cy="28.5" r="4.5" fill="var(--accent)" stroke="var(--surface)" strokeWidth="2" />
      </svg>
      {!compact && <span className="text-lg font-bold tracking-tight">MyPost</span>}
    </Link>
  );
}
