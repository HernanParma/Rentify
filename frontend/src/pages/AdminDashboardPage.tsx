import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { Reservation, Vehicle } from '../types'

export default function AdminDashboardPage() {
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([api.getAllReservations(), api.getAllVehicles()])
      .then(([res, veh]) => {
        setReservations(res)
        setVehicles(veh)
      })
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <p className="admin-loading">Cargando dashboard...</p>

  const active = reservations.filter((r) => r.reservationStatusName === 'Active').length
  const confirmed = reservations.filter((r) => r.reservationStatusName === 'Confirmed').length
  const available = vehicles.filter((v) => v.vehicleStatusName === 'Available').length
  const revenue = reservations
    .filter((r) => r.reservationStatusId !== 5 && r.reservationStatusId !== 1)
    .reduce((sum, r) => sum + r.totalCost, 0)

  return (
    <>
      <h1>Dashboard</h1>
      <div className="admin-stats">
        <div className="admin-stat-card">
          <div className="label">Reservas totales</div>
          <div className="value">{reservations.length}</div>
        </div>
        <div className="admin-stat-card">
          <div className="label">Alquileres activos</div>
          <div className="value">{active}</div>
        </div>
        <div className="admin-stat-card">
          <div className="label">Por retirar</div>
          <div className="value">{confirmed}</div>
        </div>
        <div className="admin-stat-card">
          <div className="label">Vehículos disponibles</div>
          <div className="value">{available}</div>
        </div>
        <div className="admin-stat-card">
          <div className="label">Flota total</div>
          <div className="value">{vehicles.length}</div>
        </div>
        <div className="admin-stat-card">
          <div className="label">Ingresos confirmados</div>
          <div className="value" style={{ fontSize: '1.4rem' }}>${revenue.toLocaleString('es-AR')}</div>
        </div>
      </div>
    </>
  )
}
