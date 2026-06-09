import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api } from '../services/api'
import type { Reservation } from '../types'
import './CheckoutPage.css'

export default function CheckoutPage() {
  const { reservationId } = useParams<{ reservationId: string }>()
  const navigate = useNavigate()
  const [reservation, setReservation] = useState<Reservation | null>(null)
  const [loading, setLoading] = useState(true)
  const [paying, setPaying] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!localStorage.getItem('rentify_token')) {
      navigate('/')
      return
    }
    if (!reservationId) return

    api.getReservation(reservationId)
      .then(setReservation)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [reservationId, navigate])

  const handlePay = async () => {
    if (!reservation) return
    setPaying(true)
    setError('')
    try {
      const result = await api.createPayment({
        reservationId: reservation.reservationId,
        userId: reservation.userId,
        startTime: reservation.startTime,
        endTime: reservation.endTime,
        actualPickupTime: reservation.actualPickupTime,
        actualReturnTime: reservation.actualReturnTime,
        hourlyRateSnapshot: reservation.hourlyRateSnapshot,
      })
      window.location.href = result.checkoutUrl
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al iniciar el pago')
      setPaying(false)
    }
  }

  if (loading) return <div className="checkout-loading">Cargando reserva...</div>
  if (!reservation) return <div className="checkout-error">{error || 'Reserva no encontrada'}</div>

  const start = new Date(reservation.startTime)
  const end = new Date(reservation.endTime)

  return (
    <div className="checkout-page">
      <header className="checkout-header">
        <Link to="/mapa">← Volver al mapa</Link>
        <h1>Confirmar y pagar</h1>
      </header>

      <div className="checkout-card">
        <h2>Resumen de tu reserva</h2>
        <div className="checkout-detail">
          <span>Estado</span>
          <strong>{reservation.reservationStatusName}</strong>
        </div>
        <div className="checkout-detail">
          <span>Retiro</span>
          <strong>{reservation.pickupBranchOfficeName}</strong>
        </div>
        <div className="checkout-detail">
          <span>Devolución</span>
          <strong>{reservation.dropOffBranchOfficeName}</strong>
        </div>
        <div className="checkout-detail">
          <span>Desde</span>
          <strong>{start.toLocaleString('es-AR')}</strong>
        </div>
        <div className="checkout-detail">
          <span>Hasta</span>
          <strong>{end.toLocaleString('es-AR')}</strong>
        </div>
        <div className="checkout-detail">
          <span>Tarifa horaria</span>
          <strong>${reservation.hourlyRateSnapshot.toLocaleString('es-AR')}</strong>
        </div>
        <div className="checkout-total">
          <span>Total a pagar</span>
          <strong>${reservation.totalCost.toLocaleString('es-AR')}</strong>
        </div>

        {error && <p className="checkout-error-msg">{error}</p>}

        <button className="pay-btn" onClick={handlePay} disabled={paying || reservation.reservationStatusId !== 1}>
          {paying ? 'Redirigiendo a Mercado Pago...' : 'Pagar con Mercado Pago'}
        </button>

        {reservation.reservationStatusId !== 1 && (
          <p className="checkout-note">Esta reserva ya fue procesada.</p>
        )}

        <p className="checkout-note">
          Serás redirigido a Mercado Pago para completar el pago de forma segura.
        </p>
      </div>
    </div>
  )
}
