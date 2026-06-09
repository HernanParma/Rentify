import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { getStoredUser } from '../utils/jwt'
import './AdminPage.css'

export default function AdminLayout() {
  const location = useLocation()
  const navigate = useNavigate()
  const user = getStoredUser()

  const handleLogout = () => {
    localStorage.removeItem('rentify_token')
    localStorage.removeItem('rentify_refresh')
    localStorage.removeItem('rentify_user')
    navigate('/')
  }

  const isActive = (path: string) => location.pathname === path

  return (
    <div className="admin-layout">
      <nav className="admin-nav">
        <Link to="/admin" className="admin-nav-brand">Rentify Admin</Link>
        <div className="admin-nav-links">
          {user?.role === 'Admin' && (
            <Link to="/admin" className={isActive('/admin') ? 'active' : ''}>Dashboard</Link>
          )}
          <Link to="/admin/reservas" className={isActive('/admin/reservas') ? 'active' : ''}>Reservas</Link>
          {user?.role === 'Admin' && (
            <>
              <Link to="/admin/flota" className={isActive('/admin/flota') ? 'active' : ''}>Flota</Link>
              <Link to="/admin/sucursales" className={isActive('/admin/sucursales') ? 'active' : ''}>Sucursales</Link>
              <Link to="/admin/usuarios" className={isActive('/admin/usuarios') ? 'active' : ''}>Usuarios</Link>
              <Link to="/admin/notificaciones" className={isActive('/admin/notificaciones') ? 'active' : ''}>Notificaciones</Link>
            </>
          )}
        </div>
        <span className="admin-nav-user">{user?.email} ({user?.role})</span>
        <button className="admin-nav-logout" onClick={handleLogout}>Salir</button>
      </nav>
      <main className="admin-content">
        <Outlet />
      </main>
    </div>
  )
}