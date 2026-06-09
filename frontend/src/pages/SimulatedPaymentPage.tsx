import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { api } from '../services/api'
import './PaymentResultPage.css'

export default function SimulatedPaymentPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const localPaymentId = searchParams.get('localPaymentId')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const handleConfirm = async () => {
    if (!localPaymentId) {
      setError('Identificador de pago no válido.')
      return
    }

    setLoading(true)
    setError('')
    try {
      const result = await api.completeMockPayment(localPaymentId)
      navigate('/pago/exito', {
        replace: true,
        state: {
          mockConfirmed: true,
          amount: result.amount,
          reservationId: result.reservationId,
        },
      })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al confirmar el pago simulado')
      setLoading(false)
    }
  }

  return (
    <div className="payment-result pending">
      <div className="result-card">
        <div className="result-icon">MP</div>
        <h1>Pago simulado (desarrollo)</h1>
        <p>
          Mercado Pago no está configurado con un token válido. Podés confirmar un pago de prueba
          para completar el flujo sin salir de la app.
        </p>
        {error && <p className="checkout-error-msg">{error}</p>}
        <div className="result-actions">
          <button type="button" className="btn-primary" onClick={handleConfirm} disabled={loading || !localPaymentId}>
            {loading ? 'Confirmando...' : 'Confirmar pago simulado'}
          </button>
          <Link to="/reservas" className="btn-secondary">Cancelar</Link>
        </div>
      </div>
    </div>
  )
}
