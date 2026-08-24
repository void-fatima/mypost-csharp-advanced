import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from './auth/protected-route';
import { AppShell } from './components/app-shell';
import { LoadingState } from './components/page-state';

const LandingPage = lazy(() => import('./pages/public/landing-page'));
const TrackingPage = lazy(() => import('./pages/public/tracking-page'));
const LoginPage = lazy(() => import('./pages/public/login-page'));
const RegisterPage = lazy(() => import('./pages/public/register-page'));
const NotFoundPage = lazy(() => import('./pages/public/not-found-page'));
const CustomerDashboard = lazy(() => import('./pages/customer/customer-dashboard'));
const CreateShipmentPage = lazy(() => import('./pages/customer/create-shipment-page'));
const ShipmentsPage = lazy(() => import('./pages/shared/shipments-page'));
const ShipmentDetailPage = lazy(() => import('./pages/shared/shipment-detail-page'));
const AddressesPage = lazy(() => import('./pages/customer/addresses-page'));
const ProfilePage = lazy(() => import('./pages/shared/profile-page'));
const CourierDashboard = lazy(() => import('./pages/courier/courier-dashboard'));
const CourierDetailPage = lazy(() => import('./pages/courier/courier-detail-page'));
const AdminOverview = lazy(() => import('./pages/admin/admin-overview'));
const AdminShipmentDetail = lazy(() => import('./pages/admin/admin-shipment-detail'));
const UsersPage = lazy(() => import('./pages/admin/users-page'));
const AnalyticsPage = lazy(() => import('./pages/admin/analytics-page'));

export default function App() {
  return (
    <Suspense fallback={<main className="mx-auto max-w-6xl p-6"><LoadingState label="Loading page" /></main>}>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/track" element={<TrackingPage />} />
        <Route path="/track/:trackingCode" element={<TrackingPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        <Route element={<ProtectedRoute roles={['Customer']} />}>
          <Route path="/customer" element={<AppShell />}>
            <Route index element={<CustomerDashboard />} />
            <Route path="shipments" element={<ShipmentsPage mode="customer" />} />
            <Route path="shipments/:id" element={<ShipmentDetailPage mode="customer" />} />
            <Route path="create-shipment" element={<CreateShipmentPage />} />
            <Route path="addresses" element={<AddressesPage />} />
            <Route path="profile" element={<ProfilePage />} />
          </Route>
        </Route>

        <Route element={<ProtectedRoute roles={['Courier']} />}>
          <Route path="/courier" element={<AppShell />}>
            <Route index element={<CourierDashboard />} />
            <Route path="shipments/:id" element={<CourierDetailPage />} />
            <Route path="profile" element={<ProfilePage />} />
          </Route>
        </Route>

        <Route element={<ProtectedRoute roles={['Admin']} />}>
          <Route path="/admin" element={<AppShell />}>
            <Route index element={<AdminOverview />} />
            <Route path="shipments" element={<ShipmentsPage mode="admin" />} />
            <Route path="shipments/:id" element={<AdminShipmentDetail />} />
            <Route path="users" element={<UsersPage />} />
            <Route path="analytics" element={<AnalyticsPage />} />
          </Route>
        </Route>
        <Route path="/dashboard" element={<Navigate to="/customer" replace />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  );
}
