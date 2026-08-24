import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { vi } from 'vitest';
import { ProtectedRoute } from './protected-route';

const auth = vi.hoisted(() => ({ user: null as null | { id: string; email: string; displayName: string; role: 'Customer' | 'Courier' | 'Admin' }, isLoading: false }));
vi.mock('./auth-context', () => ({ useAuth: () => ({ ...auth }) }));

describe('ProtectedRoute', () => {
  it('redirects signed-out visitors to login', () => {
    auth.user = null;
    render(<MemoryRouter initialEntries={['/customer']}><Routes><Route path="/login" element={<p>Sign in route</p>} /><Route element={<ProtectedRoute roles={['Customer']} />}><Route path="/customer" element={<p>Private route</p>} /></Route></Routes></MemoryRouter>);
    expect(screen.getByText('Sign in route')).toBeInTheDocument();
  });

  it('allows only the required role', () => {
    auth.user = { id: '1', email: 'customer@example.com', displayName: 'Customer', role: 'Customer' };
    render(<MemoryRouter initialEntries={['/customer']}><Routes><Route element={<ProtectedRoute roles={['Customer']} />}><Route path="/customer" element={<p>Private route</p>} /></Route></Routes></MemoryRouter>);
    expect(screen.getByText('Private route')).toBeInTheDocument();
  });
});
