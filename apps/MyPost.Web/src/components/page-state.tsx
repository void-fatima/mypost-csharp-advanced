import { AlertCircle, Inbox, RefreshCw } from 'lucide-react';
import { Button, Card } from './ui';

export function LoadingState({ label = 'Loading' }: { label?: string }) {
  return (
    <div className="space-y-4" aria-busy="true" aria-label={label}>
      <span className="sr-only">{label}</span>
      <div className="skeleton h-24 rounded-card" />
      <div className="skeleton h-52 rounded-card" />
    </div>
  );
}

export function EmptyState({ title, description, action }: { title: string; description: string; action?: React.ReactNode }) {
  return (
    <Card className="flex min-h-64 flex-col items-center justify-center p-8 text-center">
      <span className="mb-4 grid size-12 place-items-center rounded-full bg-subtle text-muted"><Inbox aria-hidden="true" /></span>
      <h2 className="text-lg font-bold">{title}</h2>
      <p className="mt-2 max-w-md text-sm leading-6 text-muted">{description}</p>
      {action && <div className="mt-5">{action}</div>}
    </Card>
  );
}

export function ErrorState({ error, onRetry }: { error: unknown; onRetry?: () => void }) {
  return (
    <Card className="flex min-h-56 flex-col items-center justify-center p-8 text-center" role="alert">
      <AlertCircle className="mb-4 text-danger" aria-hidden="true" />
      <h2 className="font-bold">We could not load this view</h2>
      <p className="mt-2 max-w-md text-sm text-muted">{error instanceof Error ? error.message : 'Please try again.'}</p>
      {onRetry && <Button className="mt-5" variant="secondary" onClick={onRetry}><RefreshCw className="size-4" aria-hidden="true" />Retry</Button>}
    </Card>
  );
}
