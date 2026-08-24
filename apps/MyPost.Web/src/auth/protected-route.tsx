import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { LoadingState } from '../components/page-state';
import type { UserRole } from '../types';
import { useAuth } from './auth-context';

export function ProtectedRoute({ roles }: { roles: UserRole[] }) {
  const { user, isLoading } = useAuth();
  const location = useLocation();
  if (isLoading) return <LoadingState label="Restoring your secure session" />;
  if (!user) return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  if (!roles.includes(user.role)) return <Navigate to={`/${user.role.toLowerCase()}`} replace />;
  return <Outlet />;
}
