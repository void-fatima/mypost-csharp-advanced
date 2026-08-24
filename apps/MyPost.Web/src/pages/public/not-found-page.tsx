import { ArrowLeft } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Logo } from '../../components/logo';
import { Button } from '../../components/ui';

export default function NotFoundPage() {
  const navigate = useNavigate();
  return <main className="grid min-h-screen place-items-center bg-canvas px-4 text-center"><div><Logo /><p className="mt-12 text-sm font-bold uppercase tracking-[0.18em] text-accent">404 · Route not found</p><h1 className="mt-3 text-4xl font-bold">This shipment route ends here.</h1><p className="mx-auto mt-4 max-w-md leading-7 text-muted">The page may have moved or the address may be incomplete.</p><Button className="mt-7" onClick={() => navigate(-1)}><ArrowLeft className="size-4" />Go back</Button></div></main>;
}
