import type { AuthResponse, ProblemDetails } from '../types';

const baseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:8080/api/v1';
let accessToken: string | null = null;
let refreshPromise: Promise<AuthResponse> | null = null;

export class ApiError extends Error {
  constructor(public readonly status: number, public readonly problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? 'The request could not be completed.');
  }
}

export const authToken = {
  set: (value: string | null) => { accessToken = value; },
  clear: () => { accessToken = null; },
};

async function parseError(response: Response): Promise<ApiError> {
  const problem = await response.json().catch(() => ({ title: response.statusText })) as ProblemDetails;
  return new ApiError(response.status, problem);
}

async function refreshAccess(): Promise<AuthResponse> {
  if (!refreshPromise) {
    refreshPromise = fetch(`${baseUrl}/auth/refresh`, { method: 'POST', credentials: 'include' })
      .then(async (response) => {
        if (!response.ok) throw await parseError(response);
        const session = await response.json() as AuthResponse;
        accessToken = session.accessToken;
        return session;
      })
      .finally(() => { refreshPromise = null; });
  }
  return refreshPromise;
}

async function request<T>(path: string, init: RequestInit = {}, retry = true): Promise<T> {
  const headers = new Headers(init.headers);
  if (init.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json');
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`);
  const response = await fetch(`${baseUrl}${path}`, { ...init, headers, credentials: 'include' });
  if (response.status === 401 && retry && !path.startsWith('/auth/')) {
    await refreshAccess();
    return request<T>(path, init, false);
  }
  if (!response.ok) throw await parseError(response);
  return response.status === 204 ? undefined as T : response.json() as Promise<T>;
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body?: unknown) => request<T>(path, { method: 'POST', body: body === undefined ? undefined : JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) => request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (path: string) => request<void>(path, { method: 'DELETE' }),
  refresh: refreshAccess,
};
