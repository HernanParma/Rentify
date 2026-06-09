import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../services/api'
import { getHiddenReservationIds, hideReservationFromView } from '../utils/hiddenReservations'
import { getStoredUser } from '../utils/jwt'
import type { Reservation } from '../types'
import './MyReservationsPage.css'

function TrashIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <path
        d="M3 6h18M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2m3 0v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6h14ZM10 11v6M14 11v6"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

const statusColors: Record<string, string> = {
  Pending: '#f59e0b',
  Confirmed: '#10b981',
  Active: '#3b82f6',
  Completed: '#6b7280',
  Cancelled: '#ef4444',
}

export default function MyReservationsPage() {
  const navigate = useNavigate()
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [cancellingId, setCancellingId] = useState<string | null>(null)
  const [hiddenIds, setHiddenIds] = useState<Set<string>>(new Set())

  useEffect(() => {
    const user = getStoredUser()
    if (!user) {
      navigate('/')
      return
    }

    setHiddenIds(getHiddenReservationIds(user.userId))

    api.getUserReservations(user.userId)
      .then(setReservations)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [navigate])

  const visibleReservations = useMemo(
    () => reservations.filter((r) => !hiddenIds.has(r.reservationId)),
    [reservations, hiddenIds],
  )

  const handleRemoveFromView = (reservationId: string) => {
    const user = getStoredUser()
    if (!user) return
    if (!confirm('¿Quitar esta reserva de la lista?')) return
    hideReservationFromView(user.userId, reservationId)
    setHiddenIds((prev) => new Set([...prev, reservationId]))
  }

  const handleCancel = async (reservationId: string) => {
    if (!confirm('¿Cancelar esta reserva?')) return
    setCancellingId(reservationId)
    try {
      const updated = await api.cancelReservation(reservationId)
      setReservations((prev) => prev.map((r) => r.reservationId === reservationId ? updated : r))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cancelar')
    } finally {
      setCancellingId(null)
    }
  }

  return (
    <div className="reservations-page">
      <header className="reservations-header">
        <Link to="/mapa">← Mapa</Link>
        <h1>Mis reservas</h1>
      </header>

      <div className="reservations-content">
        {loading && <p>Cargando reservas...</p>}
        {error && <p className="res-error">{error}</p>}

        {!loading && visibleReservations.length === 0 && (
          <div className="res-empty">
            <p>{reservations.length > 0 ? 'No hay reservas visibles.' : 'No tenés reservas todavía.'}</p>
            <Link to="/mapa">Explorar sedes</Link>
          </div>
        )}

        {visibleReservations.map((r) => (
          <div key={r.reservationId} className="res-card">
            <div className="res-card-header">
              <span
                className="res-status"
                style={{ background: statusColors[r.reservationStatusName] || '#94a3b8' }}
              >
                {r.reservationStatusName}
              </span>
              <div className="res-card-header-actions">
                <span className="res-id">#{r.reservationId.slice(0, 8)}</span>
                <button
                  type="button"
                  className="res-remove-btn"
                  onClick={() => handleRemoveFromView(r.reservationId)}
                  aria-label="Quitar de la lista"
                  title="Quitar de la lista"
                >
                  <TrashIcon />
                </button>
              </div>
            </div>
            <p><strong>Retiro:</strong> {r.pickupBranchOfficeName}</p>
            <p><strong>Devolución:</strong> {r.dropOffBranchOfficeName}</p>
            <p><strong>Desde:</strong> {new Date(r.startTime).toLocaleString('es-AR')}</p>
            <p><strong>Hasta:</strong> {new Date(r.endTime).toLocaleString('es-AR')}</p>
            {r.actualPickupTime && (
              <p><strong>Retiro real:</strong> {new Date(r.actualPickupTime).toLocaleString('es-AR')}</p>
            )}
            {r.actualReturnTime && (
              <p><strong>Devolución real:</strong> {new Date(r.actualReturnTime).toLocaleString('es-AR')}</p>
            )}
            <p className="res-cost"><strong>Total:</strong> ${r.totalCost.toLocaleString('es-AR')}</p>
            {r.reservationStatusId === 1 && (
              <Link to={`/checkout/${r.reservationId}`} className="res-pay-link">
                Completar pago →
              </Link>
            )}
            {(r.reservationStatusId === 1 || r.reservationStatusId === 2) && (
              <button
                className="res-cancel-btn"
                onClick={() => handleCancel(r.reservationId)}
                disabled={cancellingId === r.reservationId}
              >
                {cancellingId === r.reservationId ? 'Cancelando...' : 'Cancelar reserva'}
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}
