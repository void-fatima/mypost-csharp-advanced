import * as DropdownMenu from '@radix-ui/react-dropdown-menu';
import { BarChart3, BookUser, Boxes, ChevronDown, CircleUserRound, LayoutDashboard, LogOut, MapPinHouse, Moon, PackagePlus, Search, Sun, Truck } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/auth-context';
import type { UserRole } from '../types';
import { Logo } from './logo';
import { Button } from './ui';

const navigation: Record<UserRole, { to: string; label: string; icon: typeof Boxes }[]> = {
  Customer: [
    { to: '/customer', label: 'Dashboard', icon: LayoutDashboard },
    { to: '/customer/shipments', label: 'Shipments', icon: Boxes },
    { to: '/customer/create-shipment', label: 'Create', icon: PackagePlus },
    { to: '/customer/addresses', label: 'Addresses', icon: MapPinHouse },
    { to: '/customer/profile', label: 'Profile', icon: CircleUserRound },
  ],
  Courier: [
    { to: '/courier', label: 'Deliveries', icon: Truck },
    { to: '/courier/profile', label: 'Profile', icon: CircleUserRound },
  ],
  Admin: [
    { to: '/admin', label: 'Overview', icon: LayoutDashboard },
    { to: '/admin/shipments', label: 'Shipments', icon: Boxes },
    { to: '/admin/users', label: 'Users', icon: BookUser },
    { to: '/admin/analytics', label: 'Analytics', icon: BarChart3 },
  ],
};

export function AppShell() {
  const { user, logout } = useAuth();
  const location = useLocation();
  const [dark, setDark] = useState(() => document.documentElement.classList.contains('dark'));
  useEffect(() => { document.documentElement.classList.toggle('dark', dark); }, [dark]);
  if (!user) return null;
  const links = navigation[user.role];
  const segments = location.pathname.split('/').filter(Boolean);

  return (
    <div className="min-h-screen bg-canvas text-ink">
      <a href="#main-content" className="fixed left-3 top-3 z-50 -translate-y-20 rounded-control bg-brand px-4 py-3 text-white focus:translate-y-0">Skip to content</a>
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-66 border-r border-line bg-surface p-5 lg:flex lg:flex-col">
        <Logo />
        <p className="mt-8 px-3 text-xs font-bold uppercase tracking-[0.16em] text-muted">{user.role} workspace</p>
        <nav className="mt-3 space-y-1" aria-label="Primary">
          {links.map(({ to, label, icon: Icon }) => (
            <NavLink key={to} to={to} end={to.split('/').length === 2} className={({ isActive }) => `flex min-h-11 items-center gap-3 rounded-control px-3 text-sm font-semibold transition-colors ${isActive ? 'bg-brand text-white dark:text-canvas' : 'text-muted hover:bg-subtle hover:text-ink'}`}>
              <Icon className="size-5" aria-hidden="true" />{label}
            </NavLink>
          ))}
        </nav>
        <div className="mt-auto rounded-card border border-line bg-subtle p-4">
          <p className="text-xs font-bold uppercase tracking-wider text-accent">Virtual platform</p>
          <p className="mt-2 text-xs leading-5 text-muted">Operations and tracking in this portfolio project are simulated.</p>
        </div>
      </aside>

      <div className="lg:pl-66">
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b border-line bg-surface/95 px-4 backdrop-blur sm:px-6 lg:px-8">
          <div className="lg:hidden"><Logo /></div>
          <nav className="hidden items-center gap-2 text-xs text-muted lg:flex" aria-label="Breadcrumb">
            <Link to={`/${user.role.toLowerCase()}`}>{user.role}</Link>
            {segments.slice(1).map((segment) => <span key={segment}>/ <span className="capitalize text-ink">{segment.replaceAll('-', ' ')}</span></span>)}
          </nav>
          <div className="flex items-center gap-1">
            <Button variant="ghost" className="size-11 px-0" onClick={() => setDark((value) => !value)} aria-label={dark ? 'Use light theme' : 'Use dark theme'}>{dark ? <Sun className="size-5" /> : <Moon className="size-5" />}</Button>
            <DropdownMenu.Root>
              <DropdownMenu.Trigger asChild><Button variant="ghost" className="gap-2"><span className="hidden sm:inline">{user.displayName}</span><ChevronDown className="size-4" aria-hidden="true" /></Button></DropdownMenu.Trigger>
              <DropdownMenu.Portal>
                <DropdownMenu.Content align="end" sideOffset={8} className="z-50 min-w-56 rounded-card border border-line bg-surface p-1 shadow-xl">
                  <div className="border-b border-line px-3 py-2"><p className="text-sm font-semibold">{user.displayName}</p><p className="text-xs text-muted">{user.email}</p></div>
                  <DropdownMenu.Item asChild><button className="flex min-h-11 w-full items-center gap-2 rounded-control px-3 text-sm text-danger hover:bg-subtle" onClick={() => void logout()}><LogOut className="size-4" />Sign out</button></DropdownMenu.Item>
                </DropdownMenu.Content>
              </DropdownMenu.Portal>
            </DropdownMenu.Root>
          </div>
        </header>
        <main id="main-content" className="mx-auto max-w-[1440px] space-y-8 px-4 py-6 pb-24 sm:px-6 lg:px-8 lg:pb-10"><Outlet /></main>
      </div>

      <nav className="fixed inset-x-0 bottom-0 z-30 grid border-t border-line bg-surface px-1 pb-[env(safe-area-inset-bottom)] lg:hidden" style={{ gridTemplateColumns: `repeat(${Math.min(links.length, 5)}, minmax(0, 1fr))` }} aria-label="Mobile navigation">
        {links.slice(0, 5).map(({ to, label, icon: Icon }) => <NavLink key={to} to={to} end={to.split('/').length === 2} className={({ isActive }) => `flex min-h-16 flex-col items-center justify-center gap-1 text-[11px] font-semibold ${isActive ? 'text-brand' : 'text-muted'}`}><Icon className="size-5" aria-hidden="true" />{label}</NavLink>)}
      </nav>
    </div>
  );
}

export function SearchField({ value, onChange, placeholder = 'Search shipments' }: { value: string; onChange: (value: string) => void; placeholder?: string }) {
  return <label className="relative block"><span className="sr-only">Search</span><Search className="pointer-events-none absolute left-3 top-3 size-5 text-muted" aria-hidden="true" /><input className="min-h-11 w-full rounded-control border border-line bg-surface pl-10 pr-3 text-sm" value={value} onChange={(event) => onChange(event.target.value)} placeholder={placeholder} type="search" /></label>;
}
