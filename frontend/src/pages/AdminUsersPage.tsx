import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { AdminUser } from '../types'

const ROLES = ['Customer', 'Employee', 'Admin']

export default function AdminUsersPage() {
  const [users, setUsers] = useState<AdminUser[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [roleFilter, setRoleFilter] = useState('')

  useEffect(() => {
    api.getAdminUsers()
      .then(setUsers)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  const handleRoleChange = async (userId: number, role: string) => {
    try {
      const updated = await api.updateUserRole(userId, role)
      setUsers((prev) => prev.map((u) => u.userId === userId ? updated : u))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cambiar rol')
    }
  }

  const handleToggleActive = async (user: AdminUser) => {
    try {
      const updated = await api.updateUserStatus(user.userId, !user.isActive)
      setUsers((prev) => prev.map((u) => u.userId === user.userId ? updated : u))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cambiar estado')
    }
  }

  const filtered = users.filter((u) => {
    const matchRole = !roleFilter || u.role === roleFilter
    const term = search.toLowerCase()
    const matchSearch = !term ||
      u.email.toLowerCase().includes(term) ||
      u.firstName.toLowerCase().includes(term) ||
      u.lastName.toLowerCase().includes(term) ||
      u.dni.includes(term) ||
      String(u.userId) === term
    return matchRole && matchSearch
  })

  if (loading) return <p className="admin-loading">Cargando usuarios...</p>

  return (
    <>
      <h1>Gestión de usuarios</h1>
      {error && <p className="admin-error">{error}</p>}

      <div className="admin-filters">
        <input
          placeholder="Buscar por nombre, email, DNI o ID..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)}>
          <option value="">Todos los roles</option>
          {ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
        </select>
      </div>

      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Nombre</th>
              <th>Email</th>
              <th>DNI</th>
              <th>Rol</th>
              <th>Estado</th>
              <th>Email verificado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((u) => (
              <tr key={u.userId}>
                <td>{u.userId}</td>
                <td>{u.firstName} {u.lastName}</td>
                <td>{u.email}</td>
                <td>{u.dni}</td>
                <td>
                  <select
                    value={u.role}
                    onChange={(e) => handleRoleChange(u.userId, e.target.value)}
                    className="admin-select-inline"
                  >
                    {ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
                  </select>
                </td>
                <td>
                  <span className="admin-badge" style={{ background: u.isActive ? '#10b981' : '#ef4444' }}>
                    {u.isActive ? 'Activo' : 'Inactivo'}
                  </span>
                </td>
                <td>{u.isEmailVerified ? '✓' : '—'}</td>
                <td>
                  <button
                    className="admin-btn admin-btn-secondary"
                    onClick={() => handleToggleActive(u)}
                  >
                    {u.isActive ? 'Desactivar' : 'Activar'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  )
}
