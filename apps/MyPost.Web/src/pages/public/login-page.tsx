import { zodResolver } from '@hookform/resolvers/zod';
import { ArrowRight, LockKeyhole } from 'lucide-react';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { useAuth } from '../../auth/auth-context';
import { Logo } from '../../components/logo';
import { Button, Card, Field, Input } from '../../components/ui';

const schema = z.object({ email: z.string().email('Enter a valid email address.'), password: z.string().min(1, 'Password is required.') });
type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const { login, user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [serverError, setServerError] = useState('');
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({ resolver: zodResolver(schema) });
  if (user) return <Navigate to={`/${user.role.toLowerCase()}`} replace />;
  const onSubmit = handleSubmit(async (values) => {
    setServerError('');
    try {
      const profile = await login(values.email, values.password);
      const requested = (location.state as { from?: string } | null)?.from;
      navigate(requested?.startsWith(`/${profile.role.toLowerCase()}`) ? requested : `/${profile.role.toLowerCase()}`, { replace: true });
    } catch (error) { setServerError(error instanceof Error ? error.message : 'Sign-in failed.'); }
  });
  return <AuthLayout title="Welcome back" description="Sign in to your role-aware MyPost workspace."><form className="space-y-5" onSubmit={onSubmit} noValidate><Field label="Email" htmlFor="email" error={errors.email?.message}><Input id="email" type="email" autoComplete="email" aria-invalid={Boolean(errors.email)} aria-describedby={errors.email ? 'email-message' : undefined} {...register('email')} /></Field><Field label="Password" htmlFor="password" error={errors.password?.message}><Input id="password" type="password" autoComplete="current-password" aria-invalid={Boolean(errors.password)} aria-describedby={errors.password ? 'password-message' : undefined} {...register('password')} /></Field>{serverError && <p className="rounded-control bg-danger/10 p-3 text-sm text-danger" role="alert">{serverError}</p>}<Button className="w-full" type="submit" disabled={isSubmitting}>{isSubmitting ? 'Signing in…' : <>Sign in<ArrowRight className="size-4" /></>}</Button><p className="text-center text-sm text-muted">New to MyPost? <Link className="font-semibold text-brand hover:underline" to="/register">Create an account</Link></p></form></AuthLayout>;
}

export function AuthLayout({ title, description, children }: React.PropsWithChildren<{ title: string; description: string }>) {
  return <div className="grid min-h-screen bg-canvas lg:grid-cols-[1fr_1fr]"><section className="hidden border-r border-line bg-brand p-12 text-white dark:text-canvas lg:flex lg:flex-col"><Logo /><div className="my-auto max-w-lg"><LockKeyhole className="size-10" /><h2 className="mt-7 text-4xl font-bold leading-tight">Operational trust begins with clear access boundaries.</h2><p className="mt-5 leading-7 opacity-85">Customer records, courier assignments, and administration controls remain separated by server-enforced roles and ownership.</p></div><p className="text-xs opacity-75">Virtual portfolio platform — development data only.</p></section><main className="flex items-center justify-center px-4 py-10 sm:px-6"><Card className="w-full max-w-md p-6 sm:p-8"><div className="mb-7 lg:hidden"><Logo /></div><h1 className="text-3xl font-bold tracking-tight">{title}</h1><p className="mt-2 text-sm leading-6 text-muted">{description}</p><div className="mt-7">{children}</div></Card></main></div>;
}
