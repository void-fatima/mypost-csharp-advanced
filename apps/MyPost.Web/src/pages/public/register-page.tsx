import { zodResolver } from '@hookform/resolvers/zod';
import { ArrowRight } from 'lucide-react';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { useAuth } from '../../auth/auth-context';
import { Button, Field, Input } from '../../components/ui';
import { AuthLayout } from './login-page';

const schema = z.object({
  displayName: z.string().trim().min(2, 'Enter your name.').max(160),
  email: z.string().email('Enter a valid email address.'),
  password: z.string().min(12, 'Use at least 12 characters.').regex(/[A-Z]/, 'Add an uppercase letter.').regex(/[a-z]/, 'Add a lowercase letter.').regex(/[0-9]/, 'Add a number.').regex(/[^A-Za-z0-9]/, 'Add a symbol.'),
});
type FormValues = z.infer<typeof schema>;

export default function RegisterPage() {
  const { register: createAccount, user } = useAuth();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState('');
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({ resolver: zodResolver(schema) });
  if (user) return <Navigate to={`/${user.role.toLowerCase()}`} replace />;
  const submit = handleSubmit(async (values) => {
    setServerError('');
    try { await createAccount(values.email, values.password, values.displayName); navigate('/customer', { replace: true }); }
    catch (error) { setServerError(error instanceof Error ? error.message : 'Registration failed.'); }
  });
  return <AuthLayout title="Create your account" description="Start a private customer workspace for virtual shipments."><form className="space-y-5" onSubmit={submit} noValidate><Field label="Full name" htmlFor="displayName" error={errors.displayName?.message}><Input id="displayName" autoComplete="name" aria-invalid={Boolean(errors.displayName)} {...register('displayName')} /></Field><Field label="Email" htmlFor="register-email" error={errors.email?.message}><Input id="register-email" type="email" autoComplete="email" aria-invalid={Boolean(errors.email)} {...register('email')} /></Field><Field label="Password" htmlFor="register-password" hint="12+ characters with upper, lower, number, and symbol." error={errors.password?.message}><Input id="register-password" type="password" autoComplete="new-password" aria-invalid={Boolean(errors.password)} {...register('password')} /></Field>{serverError && <p className="rounded-control bg-danger/10 p-3 text-sm text-danger" role="alert">{serverError}</p>}<Button className="w-full" type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating account…' : <>Create account<ArrowRight className="size-4" /></>}</Button><p className="text-center text-sm text-muted">Already registered? <Link className="font-semibold text-brand hover:underline" to="/login">Sign in</Link></p></form></AuthLayout>;
}
