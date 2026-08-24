import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { EmptyState, ErrorState, LoadingState } from './page-state';

describe('page states', () => {
  it('announces loading and renders actionable empty and error states', async () => {
    const retry = vi.fn();
    const { rerender } = render(<LoadingState label="Loading shipments" />);
    expect(screen.getByLabelText('Loading shipments')).toHaveAttribute('aria-busy', 'true');
    rerender(<EmptyState title="No shipments" description="Create the first shipment." />);
    expect(screen.getByRole('heading', { name: 'No shipments' })).toBeInTheDocument();
    rerender(<ErrorState error={new Error('Network unavailable')} onRetry={retry} />);
    expect(screen.getByRole('alert')).toHaveTextContent('Network unavailable');
    await userEvent.click(screen.getByRole('button', { name: /retry/i }));
    expect(retry).toHaveBeenCalledOnce();
  });
});
