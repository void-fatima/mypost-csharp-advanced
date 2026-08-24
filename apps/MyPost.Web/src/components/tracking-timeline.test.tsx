import { render, screen } from '@testing-library/react';
import { TrackingTimeline } from './tracking-timeline';

describe('TrackingTimeline', () => {
  it('renders ordered shipment milestones with semantic timestamps', () => {
    const { container } = render(<TrackingTimeline events={[
      { status: 'Created', description: 'Shipment created', occurredAtUtc: '2026-01-01T10:00:00Z' },
      { status: 'InTransit', description: 'Departed origin', occurredAtUtc: '2026-01-02T10:00:00Z', location: 'Tehran hub' },
    ]} />);
    expect(screen.getByRole('list', { name: /shipment history/i })).toBeInTheDocument();
    expect(screen.getByText('Departed origin')).toBeInTheDocument();
    expect(screen.getByText(/Tehran hub/)).toBeInTheDocument();
    expect(container.querySelectorAll('time')).toHaveLength(2);
  });
});
