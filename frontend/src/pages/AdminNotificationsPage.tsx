import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { NotificationRecord } from '../types'

const STATUS_OPTIONS = ['', 'Pending', 'Sent', 'Failed']

const statusColors: Record<string, string> = {
  Pending: '#f59e0b',
  Sent: '#10b981',
  Failed: '#ef4444',
  Read: '#6b7280',
}

export default function AdminNotificationsPage() {
  const [notifications, setNotifications] = useState<NotificationRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [userIdFilter, setUserIdFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')

  const load = () => {
    setLoading(true)
    api.getNotificationHistory({
      userId: userIdFilter ? +userIdFilter : undefined,
      status: statusFilter || undefined,
      limit: 200,
    })
      .then(setNotifications)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }

  useEffect(load, [userIdFilter, statusFilter])

  return (
    <>
      <h1>Historial de notificaciones</h1>
      {error && <p className="admin-error">{error}</p>}

      <div className="admin-filters">
        <input
          type="number"
          placeholder="Filtrar por User ID"
          value={userIdFilter}
          onChange={(e) => setUserIdFilter(e.target.value)}
        />
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
          {STATUS_OPTIONS.map((s) => (
            <option key={s} value={s}>{s || 'Todos los estados'}</option>
          ))}
        </select>
        <button className="admin-btn admin-btn-secondary" onClick={load}>Actualizar</button>
      </div>

      {loading ? (
        <p className="admin-loading">Cargando...</p>
      ) : (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Usuario</th>
                <th>Tipo</th>
                <th>Estado</th>
                <th>Enviado</th>
              </tr>
            </thead>
            <tbody>
              {notifications.length === 0 && (
                <tr><td colSpan={5} style={{ textAlign: 'center', color: '#94a3b8' }}>Sin notificaciones</td></tr>
              )}
              {notifications.map((n) => (
                <tr key={n.notificationId}>
                  <td>{new Date(n.createdAt).toLocaleString('es-AR')}</td>
                  <td>{n.userEmail || `ID ${n.userId}`}</td>
                  <td>{n.type}</td>
                  <td>
                    <span className="admin-badge" style={{ background: statusColors[n.status] || '#94a3b8' }}>
                      {n.status}
                    </span>
                  </td>
                  <td>{n.sentAt ? new Date(n.sentAt).toLocaleString('es-AR') : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  )
}
