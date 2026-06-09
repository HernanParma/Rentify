import { Routes, Route, Navigate } from 'react-router-dom'
import LoginPage from './pages/LoginPage'
import MapPage from './pages/MapPage'
import CheckoutPage from './pages/CheckoutPage'
import PaymentResultPage from './pages/PaymentResultPage'
import SimulatedPaymentPage from './pages/SimulatedPaymentPage'
import MyReservationsPage from './pages/MyReservationsPage'
import AdminLayout from './pages/AdminLayout'
import AdminDashboardPage from './pages/AdminDashboardPage'
import AdminReservationsPage from './pages/AdminReservationsPage'
import AdminFleetPage from './pages/AdminFleetPage'
import AdminBranchesPage from './pages/AdminBranchesPage'
import AdminUsersPage from './pages/AdminUsersPage'
import AdminNotificationsPage from './pages/AdminNotificationsPage'
import { getStoredUser } from './utils/jwt'

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const token = localStorage.getItem('rentify_token')
  return token ? <>{children}</> : <Navigate to="/" replace />
}

function AdminRoute({ children, adminOnly = false }: { children: React.ReactNode; adminOnly?: boolean }) {
  const user = getStoredUser()
  if (!user) return <Navigate to="/" replace />
  if (user.role !== 'Admin' && user.role !== 'Employee') return <Navigate to="/mapa" replace />
  if (adminOnly && user.role !== 'Admin') return <Navigate to="/admin/reservas" replace />
  return <>{children}</>
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<LoginPage />} />
      <Route path="/mapa" element={<PrivateRoute><MapPage /></PrivateRoute>} />
      <Route path="/checkout/:reservationId" element={<PrivateRoute><CheckoutPage /></PrivateRoute>} />
      <Route path="/reservas" element={<PrivateRoute><MyReservationsPage /></PrivateRoute>} />
      <Route path="/pago/simulado" element={<PrivateRoute><SimulatedPaymentPage /></PrivateRoute>} />
      <Route path="/pago/exito" element={<PaymentResultPage status="success" />} />
      <Route path="/pago/fallo" element={<PaymentResultPage status="failure" />} />
      <Route path="/pago/pendiente" element={<PaymentResultPage status="pending" />} />

      <Route path="/admin" element={<AdminRoute><AdminLayout /></AdminRoute>}>
        <Route index element={<AdminRoute adminOnly><AdminDashboardPage /></AdminRoute>} />
        <Route path="reservas" element={<AdminReservationsPage />} />
        <Route path="flota" element={<AdminRoute adminOnly><AdminFleetPage /></AdminRoute>} />
        <Route path="sucursales" element={<AdminRoute adminOnly><AdminBranchesPage /></AdminRoute>} />
        <Route path="usuarios" element={<AdminRoute adminOnly><AdminUsersPage /></AdminRoute>} />
        <Route path="notificaciones" element={<AdminRoute adminOnly><AdminNotificationsPage /></AdminRoute>} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
