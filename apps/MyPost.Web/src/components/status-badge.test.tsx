import { render, screen } from '@testing-library/react';
import { StatusBadge } from './status-badge';

describe('StatusBadge', () => {
  it('communicates status with readable text, not color alone', () => {
    render(<StatusBadge status="DeliveryFailed" />);
    expect(screen.getByText('Delivery failed')).toBeInTheDocument();
    expect(screen.getByText('Delivery failed').querySelector('svg')).toBeInTheDocument();
  });
});
