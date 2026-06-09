import { useEffect, useState } from 'react'
import { Link, useLocation, useSearchParams } from 'react-router-dom'
import { api } from '../services/api'
import './PaymentResultPage.css'

interface Props {
  status: 'success' | 'failure' | 'pending'
}

interface MockConfirmState {
  mockConfirmed?: boolean
  amount?: number
  reservationId?: string
}

export default function PaymentResultPage({ status }: Props) {
  const [searchParams] = useSearchParams()
  const location = useLocation()
  const mockState = location.state as MockConfirmState | null
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(status === 'success')
  const [verified, setVerified] = useState(false)

  useEffect(() => {
    if (status !== 'success') return

    if (mockState?.mockConfirmed) {
      setVerified(true)
      setMessage(
        `Pago simulado confirmado. Monto: $${mockState.amount?.toLocaleString('es-AR') ?? '—'}`,
      )
      setLoading(false)
      return
    }

    const paymentId = searchParams.get('payment_id') || searchParams.get('collection_id')
    if (!paymentId) {
      setMessage('Pago recibido. Revisá tus reservas para confirmar el estado.')
      setLoading(false)
      return
    }

    api.verifyPayment(Number(paymentId))
      .then((result) => {
        setVerified(true)
        setMessage(`Pago confirmado. Monto: $${(result.amount as number)?.toLocaleString('es-AR') ?? '—'}`)
      })
      .catch(() => {
        setMessage('El pago fue procesado por Mercado Pago. Revisá "Mis reservas" para ver el estado actualizado.')
      })
      .finally(() => setLoading(false))
  }, [status, searchParams, mockState])

  const titles = {
    success: '¡Pago exitoso!',
    failure: 'Pago rechazado',
    pending: 'Pago pendiente',
  }

  const icons = { success: '✓', failure: '✕', pending: '…' }

  return (
    <div className={`payment-result ${status}`}>
      <div className="result-card">
        <div className="result-icon">{icons[status]}</div>
        <h1>{titles[status]}</h1>

        {loading ? (
          <p>Verificando pago...</p>
        ) : (
          <p>{message || (status === 'failure' ? 'El pago no pudo completarse. Intentá nuevamente.' : 'Tu pago está siendo procesado.')}</p>
        )}

        {verified && <p className="result-ok">Tu reserva fue confirmada.</p>}

        <div className="result-actions">
          <Link to="/reservas" className="btn-primary">Ver mis reservas</Link>
          <Link to="/mapa" className="btn-secondary">Volver al mapa</Link>
        </div>
      </div>
    </div>
  )
}
