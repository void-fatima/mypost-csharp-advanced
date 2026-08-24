import { Check } from 'lucide-react';
import type { TrackingEvent } from '../types';
import { StatusBadge } from './status-badge';

export function TrackingTimeline({ events }: { events: TrackingEvent[] }) {
  return (
    <ol className="space-y-0" aria-label="Shipment history">
      {events.map((event, index) => (
        <li key={`${event.status}-${event.occurredAtUtc}`} className="grid grid-cols-[32px_1fr] gap-3">
          <div className="flex flex-col items-center">
            <span className="grid size-8 shrink-0 place-items-center rounded-full border-2 border-brand bg-surface text-brand"><Check className="size-4" aria-hidden="true" /></span>
            {index < events.length - 1 && <span className="min-h-12 w-0.5 flex-1 bg-brand/30" aria-hidden="true" />}
          </div>
          <div className="pb-7">
            <StatusBadge status={event.status} />
            <p className="mt-2 text-sm font-semibold text-ink">{event.description}</p>
            <p className="mt-1 text-xs text-muted"><time dateTime={event.occurredAtUtc}>{new Intl.DateTimeFormat('en', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(event.occurredAtUtc))}</time>{event.location ? ` · ${event.location}` : ''}</p>
          </div>
        </li>
      ))}
    </ol>
  );
}
