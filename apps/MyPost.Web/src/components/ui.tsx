import clsx from 'clsx';
import { forwardRef, type ButtonHTMLAttributes, type ComponentPropsWithoutRef, type InputHTMLAttributes, type ReactNode } from 'react';

export const Button = forwardRef<HTMLButtonElement, ButtonHTMLAttributes<HTMLButtonElement> & { variant?: 'primary' | 'secondary' | 'danger' | 'ghost' }>(
  ({ className, variant = 'primary', type = 'button', ...props }, ref) => (
    <button
      ref={ref}
      type={type}
      className={clsx(
        'inline-flex min-h-11 items-center justify-center gap-2 rounded-control px-4 text-sm font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-50',
        variant === 'primary' && 'bg-brand text-white hover:bg-brand-strong dark:text-canvas',
        variant === 'secondary' && 'border border-line bg-surface text-ink hover:bg-subtle',
        variant === 'danger' && 'bg-danger text-white hover:brightness-90',
        variant === 'ghost' && 'text-muted hover:bg-subtle hover:text-ink',
        className,
      )}
      {...props}
    />
  ),
);
Button.displayName = 'Button';

export const Input = forwardRef<HTMLInputElement, InputHTMLAttributes<HTMLInputElement>>(({ className, ...props }, ref) => (
  <input ref={ref} className={clsx('min-h-11 w-full rounded-control border border-line bg-surface px-3 text-sm text-ink placeholder:text-muted focus:border-brand disabled:cursor-not-allowed disabled:bg-subtle', className)} {...props} />
));
Input.displayName = 'Input';

export function Select({ className, ...props }: React.SelectHTMLAttributes<HTMLSelectElement>) {
  return <select className={clsx('min-h-11 w-full rounded-control border border-line bg-surface px-3 text-sm text-ink focus:border-brand disabled:cursor-not-allowed disabled:bg-subtle', className)} {...props} />;
}

export function Textarea(props: React.TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea className="min-h-28 w-full rounded-control border border-line bg-surface px-3 py-2 text-sm text-ink focus:border-brand disabled:cursor-not-allowed disabled:bg-subtle" {...props} />;
}

export function Card({ children, className, ...props }: ComponentPropsWithoutRef<'section'>) {
  return <section className={clsx('rounded-card border border-line bg-surface', className)} {...props}>{children}</section>;
}

export function Field({ label, error, hint, children, htmlFor }: { label: string; error?: string; hint?: string; children: ReactNode; htmlFor: string }) {
  const messageId = `${htmlFor}-message`;
  return (
    <div className="space-y-1.5">
      <label className="block text-sm font-semibold text-ink" htmlFor={htmlFor}>{label}</label>
      {children}
      {(error ?? hint) && <p id={messageId} className={clsx('text-xs', error ? 'text-danger' : 'text-muted')}>{error ?? hint}</p>}
    </div>
  );
}

export function PageHeader({ eyebrow, title, description, action }: { eyebrow?: string; title: string; description?: string; action?: ReactNode }) {
  return (
    <header className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
      <div>
        {eyebrow && <p className="mb-2 text-xs font-bold uppercase tracking-[0.16em] text-brand">{eyebrow}</p>}
        <h1 className="text-3xl font-bold tracking-tight text-ink">{title}</h1>
        {description && <p className="mt-2 max-w-2xl text-sm leading-6 text-muted">{description}</p>}
      </div>
      {action}
    </header>
  );
}
