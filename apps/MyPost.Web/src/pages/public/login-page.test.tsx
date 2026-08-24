import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { vi } from 'vitest';
import LoginPage from './login-page';

const login = vi.fn();
vi.mock('../../auth/auth-context', () => ({ useAuth: () => ({ login, user: null }) }));

describe('LoginPage', () => {
  it('shows inline validation and does not submit invalid credentials', async () => {
    render(<MemoryRouter><LoginPage /></MemoryRouter>);
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }));
    expect(await screen.findByText('Enter a valid email address.')).toBeInTheDocument();
    expect(screen.getByText('Password is required.')).toBeInTheDocument();
    expect(login).not.toHaveBeenCalled();
  });
});
