import { createContext, useContext, useEffect, useMemo, useState, type PropsWithChildren } from 'react';
import { api, authToken } from '../lib/api';
import type { AuthResponse, UserProfile } from '../types';

interface AuthContextValue {
  user: UserProfile | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<UserProfile>;
  register: (email: string, password: string, displayName: string) => Promise<UserProfile>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [isLoading, setLoading] = useState(true);

  useEffect(() => {
    api.refresh().then((session) => setUser(session.user)).catch(() => authToken.clear()).finally(() => setLoading(false));
  }, []);

  async function authenticate(path: 'login' | 'register', payload: object) {
    const session = await api.post<AuthResponse>(`/auth/${path}`, payload);
    authToken.set(session.accessToken);
    setUser(session.user);
    return session.user;
  }

  const value = useMemo<AuthContextValue>(() => ({
    user,
    isLoading,
    login: (email, password) => authenticate('login', { email, password }),
    register: (email, password, displayName) => authenticate('register', { email, password, displayName }),
    logout: async () => {
      try { await api.post<void>('/auth/logout'); } finally { authToken.clear(); setUser(null); }
    },
  }), [user, isLoading]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth must be used inside AuthProvider.');
  return value;
}
