import { ArrowRight, Boxes, CheckCircle2, ClipboardCheck, LockKeyhole, PackageCheck, Route, ScanSearch, Truck } from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Logo } from '../../components/logo';
import { Button, Card } from '../../components/ui';

export default function LandingPage() {
  const [code, setCode] = useState('');
  const navigate = useNavigate();
  function track(event: React.FormEvent) {
    event.preventDefault();
    if (code.trim()) navigate(`/track/${encodeURIComponent(code.trim().toUpperCase())}`);
  }
  return (
    <div className="min-h-screen bg-canvas text-ink">
      <header className="border-b border-line bg-surface"><div className="mx-auto flex h-18 max-w-7xl items-center justify-between px-4 sm:px-6"><Logo /><nav className="flex items-center gap-2" aria-label="Public"><Link className="hidden min-h-11 items-center px-3 text-sm font-semibold text-muted hover:text-ink sm:flex" to="/track">Track</Link><Link className="hidden min-h-11 items-center px-3 text-sm font-semibold text-muted hover:text-ink sm:flex" to="/login">Sign in</Link><Button onClick={() => navigate('/register')}>Create account</Button></nav></div></header>
      <main>
        <section className="relative overflow-hidden border-b border-line bg-surface">
          <div className="route-grid pointer-events-none absolute inset-0 opacity-70" aria-hidden="true" />
          <div className="relative mx-auto grid max-w-7xl items-center gap-12 px-4 py-16 sm:px-6 lg:grid-cols-[1.05fr_.95fr] lg:py-24">
            <div>
              <p className="mb-4 text-xs font-bold uppercase tracking-[0.18em] text-accent">Virtual postal operations, clearly tracked</p>
              <h1 className="max-w-3xl text-4xl font-bold leading-tight tracking-[-0.035em] sm:text-5xl">From shipment creation to the final doorstep, every handoff stays visible.</h1>
              <p className="mt-6 max-w-2xl text-base leading-7 text-muted">MyPost brings customer shipping, courier delivery, and operational control into one focused demo platform—with explicit status rules and privacy-aware public tracking.</p>
              <form className="mt-8 flex max-w-xl flex-col gap-3 rounded-card border border-line bg-canvas p-3 sm:flex-row" onSubmit={track}>
                <label className="sr-only" htmlFor="hero-code">Tracking code</label>
                <input id="hero-code" className="min-h-12 flex-1 rounded-control border border-line bg-surface px-4 font-mono text-sm uppercase tracking-wider" value={code} onChange={(event) => setCode(event.target.value)} placeholder="Enter tracking code" />
                <Button type="submit" className="min-h-12"><ScanSearch className="size-4" />Track shipment</Button>
              </form>
              <p className="mt-3 text-xs text-muted">Try seeded code <button className="font-mono font-bold text-brand hover:underline" onClick={() => setCode('MP-DEMO-100004')}>MP-DEMO-100004</button> after starting the development seed.</p>
            </div>
            <Card className="relative overflow-hidden p-6 shadow-2xl shadow-brand/10">
              <div className="flex items-start justify-between border-b border-line pb-5"><div><p className="text-xs font-bold uppercase tracking-wider text-muted">Shipment journey</p><p className="mt-2 font-mono text-sm font-bold tracking-wider">MP-DEMO-100004</p></div><span className="rounded-full bg-success/10 px-3 py-1 text-xs font-bold text-success">✓ Delivered</span></div>
              <div className="grid grid-cols-3 gap-3 py-6"><Metric label="Origin" value="Tehran" /><Metric label="Service" value="Standard" /><Metric label="Events" value="6" /></div>
              <div className="space-y-0">{['Accepted at origin facility', 'Departed origin facility', 'Out for delivery', 'Delivered to recipient'].map((label, index) => <div key={label} className="grid grid-cols-[28px_1fr] gap-3"><div className="flex flex-col items-center"><span className={`mt-0.5 size-3 rounded-full ${index === 3 ? 'bg-success' : 'bg-brand'}`} />{index < 3 && <span className="h-10 w-0.5 bg-line" />}</div><p className="text-sm font-semibold">{label}</p></div>)}</div>
              <div className="absolute -right-12 -top-12 size-32 rounded-full border-[18px] border-accent/10" aria-hidden="true" />
            </Card>
          </div>
        </section>

        <section className="mx-auto max-w-7xl px-4 py-20 sm:px-6"><div className="mb-10 max-w-2xl"><p className="text-xs font-bold uppercase tracking-[0.16em] text-brand">One operating picture</p><h2 className="mt-3 text-3xl font-bold tracking-tight">Built around the shipment, not dashboard decoration.</h2></div><div className="grid gap-4 md:grid-cols-3"><Capability icon={Boxes} title="Customer shipping" text="Saved sender addresses, guided shipment creation, calculated pricing, and a complete private history." /><Capability icon={Truck} title="Courier workspace" text="Only assigned deliveries are visible, with constrained status and delivery-result workflows." /><Capability icon={ClipboardCheck} title="Operations control" text="Assignment, lifecycle management, return-to-sender rules, users, and status analytics in one place." /></div></section>
        <section className="border-y border-line bg-surface"><div className="mx-auto max-w-7xl px-4 py-20 sm:px-6"><h2 className="text-3xl font-bold">How it works</h2><ol className="mt-10 grid gap-8 md:grid-cols-3"><Step number="01" icon={PackageCheck} title="Create with confidence" text="Choose an address, recipient, shipment type, weight, dimensions, and service level. The server validates and prices it." /><Step number="02" icon={Route} title="Move through legal states" text="Operations and couriers record each accepted handoff. Terminal shipments cannot silently move backward." /><Step number="03" icon={CheckCircle2} title="Track the outcome" text="Customers see the private record; public visitors get a safe, intentionally limited tracking view." /></ol></div></section>
        <section className="mx-auto grid max-w-7xl gap-8 px-4 py-20 sm:px-6 lg:grid-cols-[1fr_auto] lg:items-center"><div><div className="flex items-center gap-2 text-brand"><LockKeyhole className="size-5" /><span className="text-sm font-bold">Security-shaped by default</span></div><h2 className="mt-3 text-3xl font-bold">Identity, ownership, and privacy live on the server.</h2><p className="mt-4 max-w-3xl leading-7 text-muted">Role policies are backed by resource checks, Identity password hashing, short access sessions, rotating HttpOnly refresh tokens, rate limits, and consistent problem responses.</p></div><Button onClick={() => navigate('/register')}>Start a demo account<ArrowRight className="size-4" /></Button></section>
      </main>
      <footer className="border-t border-line bg-surface"><div className="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-8 text-sm text-muted sm:flex-row sm:items-center sm:justify-between sm:px-6"><Logo /><p>Portfolio demonstration — not an external postal carrier.</p></div></footer>
    </div>
  );
}

function Metric({ label, value }: { label: string; value: string }) { return <div><p className="text-[11px] font-bold uppercase tracking-wider text-muted">{label}</p><p className="mt-1 text-sm font-bold">{value}</p></div>; }
function Capability({ icon: Icon, title, text }: { icon: typeof Boxes; title: string; text: string }) { return <Card className="p-6"><span className="grid size-11 place-items-center rounded-control bg-brand/10 text-brand"><Icon className="size-5" /></span><h3 className="mt-5 text-lg font-bold">{title}</h3><p className="mt-2 text-sm leading-6 text-muted">{text}</p></Card>; }
function Step({ number, icon: Icon, title, text }: { number: string; icon: typeof Boxes; title: string; text: string }) { return <li className="relative"><span className="text-xs font-bold tracking-widest text-accent">{number}</span><Icon className="mt-5 size-7 text-brand" /><h3 className="mt-4 text-lg font-bold">{title}</h3><p className="mt-2 text-sm leading-6 text-muted">{text}</p></li>; }
