import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { Branch, Reservation } from '../types'

const STATUS_OPTIONS = [
  { id: '', label: 'Todos' },
  { id: '1', label: 'Pending' },
  { id: '2', label: 'Confirmed' },
  { id: '3', label: 'Active' },
  { id: '4', label: 'Completed' },
  { id: '5', label: 'Cancelled' },
]

const statusColors: Record<string, string> = {
  Pending: '#f59e0b',
  Confirmed: '#10b981',
  Active: '#3b82f6',
  Completed: '#6b7280',
  Cancelled: '#ef4444',
}

function toLocalInputValue(iso?: string) {
  if (!iso) return ''
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

export default function AdminReservationsPage() {
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [branches, setBranches] = useState<Branch[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [processingId, setProcessingId] = useState<string | null>(null)
  const [timestamps, setTimestamps] = useState<Record<string, { pickup?: string; return?: string }>>({})

  const [statusId, setStatusId] = useState('')
  const [branchId, setBranchId] = useState('')
  const [userId, setUserId] = useState('')
  const [search, setSearch] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')

  const load = () => {
    setLoading(true)
    api.getAllReservations({
      statusId: statusId ? +statusId : undefined,
      branchId: branchId ? +branchId : undefined,
      userId: userId ? +userId : undefined,
      search: search || undefined,
      from: from || undefined,
      to: to || undefined,
    })
      .then(setReservations)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    api.getBranches().then(setBranches).catch(() => {})
  }, [])

  useEffect(load, [])

  const handlePickup = async (reservationId: string) => {
    setProcessingId(reservationId)
    setError('')
    try {
      const ts = timestamps[reservationId]?.pickup
      const updated = await api.registerPickup(reservationId, ts || undefined)
      setReservations((prev) => prev.map((r) => r.reservationId === reservationId ? updated : r))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al registrar retiro')
    } finally {
      setProcessingId(null)
    }
  }

  const handleReturn = async (reservationId: string) => {
    setProcessingId(reservationId)
    setError('')
    try {
      const ts = timestamps[reservationId]?.return
      const updated = await api.registerReturn(reservationId, ts || undefined)
      setReservations((prev) => prev.map((r) => r.reservationId === reservationId ? updated : r))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al registrar devolución')
    } finally {
      setProcessingId(null)
    }
  }

  return (
    <>
      <h1>Gestión de reservas</h1>
      {error && <p className="admin-error">{error}</p>}

      <div className="admin-filters">
        <input placeholder="Buscar ID reserva o user ID..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <select value={statusId} onChange={(e) => setStatusId(e.target.value)}>
          {STATUS_OPTIONS.map((s) => <option key={s.id} value={s.id}>{s.label}</option>)}
        </select>
        <select value={branchId} onChange={(e) => setBranchId(e.target.value)}>
          <option value="">Todas las sedes</option>
          {branches.map((b) => <option key={b.branchOfficeId} value={b.branchOfficeId}>{b.name}</option>)}
        </select>
        <input type="number" placeholder="User ID" value={userId} onChange={(e) => setUserId(e.target.value)} />
        <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} title="Desde" />
        <input type="date" value={to} onChange={(e) => setTo(e.target.value)} title="Hasta" />
        <button className="admin-btn admin-btn-primary" onClick={load}>Filtrar</button>
      </div>

      {loading ? (
        <p className="admin-loading">Cargando reservas...</p>
      ) : (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Cliente</th>
                <th>Estado</th>
                <th>Retiro / Devolución</th>
                <th>Hora real</th>
                <th>Total</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {reservations.length === 0 && (
                <tr><td colSpan={7} style={{ textAlign: 'center', color: '#94a3b8' }}>Sin resultados</td></tr>
              )}
              {reservations.map((r) => (
                <tr key={r.reservationId}>
                  <td>#{r.reservationId.slice(0, 8)}</td>
                  <td>Usuario {r.userId}</td>
                  <td>
                    <span className="admin-badge" style={{ background: statusColors[r.reservationStatusName] || '#94a3b8' }}>
                      {r.reservationStatusName}
                    </span>
                  </td>
                  <td>
                    <div>{r.pickupBranchOfficeName}</div>
                    <div style={{ fontSize: '0.75rem', color: '#94a3b8' }}>{new Date(r.startTime).toLocaleString('es-AR')}</div>
                    <div style={{ marginTop: '0.25rem' }}>{r.dropOffBranchOfficeName}</div>
                    <div style={{ fontSize: '0.75rem', color: '#94a3b8' }}>{new Date(r.endTime).toLocaleString('es-AR')}</div>
                  </td>
                  <td>
                    {r.actualPickupTime && <div style={{ fontSize: '0.8rem' }}>Retiro: {new Date(r.actualPickupTime).toLocaleString('es-AR')}</div>}
                    {r.actualReturnTime && <div style={{ fontSize: '0.8rem' }}>Devol.: {new Date(r.actualReturnTime).toLocaleString('es-AR')}</div>}
                    {!r.actualPickupTime && !r.actualReturnTime && '—'}
                  </td>
                  <td>${r.totalCost.toLocaleString('es-AR')}</td>
                  <td>
                    <div className="admin-actions">
                      {r.reservationStatusId === 2 && (
                        <>
                          <input type="datetime-local" className="admin-datetime"
                            value={timestamps[r.reservationId]?.pickup ?? toLocalInputValue(new Date().toISOString())}
                            onChange={(e) => setTimestamps((prev) => ({ ...prev, [r.reservationId]: { ...prev[r.reservationId], pickup: e.target.value } }))}
                          />
                          <button className="admin-btn admin-btn-pickup" disabled={processingId === r.reservationId} onClick={() => handlePickup(r.reservationId)}>
                            Registrar retiro
                          </button>
                        </>
                      )}
                      {r.reservationStatusId === 3 && (
                        <>
                          <input type="datetime-local" className="admin-datetime"
                            value={timestamps[r.reservationId]?.return ?? toLocalInputValue(new Date().toISOString())}
                            onChange={(e) => setTimestamps((prev) => ({ ...prev, [r.reservationId]: { ...prev[r.reservationId], return: e.target.value } }))}
                          />
                          <button className="admin-btn admin-btn-return" disabled={processingId === r.reservationId} onClick={() => handleReturn(r.reservationId)}>
                            Registrar devolución
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  )
}