import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, estimateCost } from '../services/api'
import AvailabilityCalendar from './AvailabilityCalendar'
import { getVehicleCatalogEntry } from '../data/vehicleCatalog'
import { getStoredUser } from '../utils/jwt'
import { findBookingConflict, formatBookedRange } from '../utils/reservationDates'
import type { BookedRange, Branch, BranchMapItem, Vehicle } from '../types'
import './BookingModal.css'

function localDateInputValue(d = new Date()) {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

interface Props {
  vehicle: Vehicle
  pickupBranch: BranchMapItem
  onClose: () => void
  initialStartDate?: string
  initialEndDate?: string
  initialStartTime?: string
  initialEndTime?: string
}

export default function BookingModal({
  vehicle,
  pickupBranch,
  onClose,
  initialStartDate,
  initialEndDate,
  initialStartTime,
  initialEndTime,
}: Props) {
  const navigate = useNavigate()
  const [branches, setBranches] = useState<Branch[]>([])
  const [bookedRanges, setBookedRanges] = useState<BookedRange[]>([])
  const [loadingRanges, setLoadingRanges] = useState(true)
  const [dropOffId, setDropOffId] = useState(pickupBranch.branchOfficeId)
  const [startDate, setStartDate] = useState('')
  const [startTime, setStartTime] = useState('10:00')
  const [endDate, setEndDate] = useState('')
  const [endTime, setEndTime] = useState('18:00')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const inMaintenance = vehicle.vehicleStatusName === 'Maintenance'

  useEffect(() => {
    api.getBranches().then(setBranches).catch(() => {})
    setLoadingRanges(true)
    api.getVehicleBookedRanges(vehicle.vehicleId)
      .then(setBookedRanges)
      .catch(() => setError('No se pudieron cargar las fechas reservadas. Verificá que ReservationMS esté activo.'))
      .finally(() => setLoadingRanges(false))

    if (initialStartDate && initialEndDate) {
      setStartDate(initialStartDate)
      setEndDate(initialEndDate)
      if (initialStartTime) setStartTime(initialStartTime)
      if (initialEndTime) setEndTime(initialEndTime)
    } else {
      const tomorrow = new Date()
      tomorrow.setDate(tomorrow.getDate() + 1)
      const dayAfter = new Date()
      dayAfter.setDate(dayAfter.getDate() + 2)
      setStartDate(localDateInputValue(tomorrow))
      setEndDate(localDateInputValue(dayAfter))
    }
  }, [vehicle.vehicleId, initialStartDate, initialEndDate, initialStartTime, initialEndTime])

  const handleRangeChange = (start: string, end: string) => {
    setStartDate(start)
    setEndDate(end)
    setError('')
  }

  const vehicleInfo = getVehicleCatalogEntry(vehicle.brand, vehicle.model)

  const start = startDate && startTime ? new Date(`${startDate}T${startTime}`) : null
  const end = endDate && endTime ? new Date(`${endDate}T${endTime}`) : null
  const conflict = start && end && end > start ? findBookingConflict(start, end, bookedRanges) : null
  const estimated = start && end && end > start && start >= new Date() && !conflict
    ? estimateCost(vehicle.pricePerDay, start, end)
    : 0

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')

    if (inMaintenance) {
      setError('Este vehículo está en mantenimiento y no puede reservarse.')
      return
    }
    if (!startDate || !endDate) {
      setError('Seleccioná fechas en el calendario (días verdes).')
      return
    }

    const user = getStoredUser()
    if (!user) {
      setError('Sesión expirada. Volvé a iniciar sesión.')
      return
    }
    if (!start || !end || end <= start) {
      setError('La fecha de fin debe ser posterior al inicio.')
      return
    }
    if (start < new Date()) {
      setError('No podés reservar con una fecha u hora de inicio en el pasado.')
      return
    }
    if (conflict) {
      setError(`El vehículo ya está reservado del ${formatBookedRange(conflict)}. Elegí otras fechas.`)
      return
    }

    setLoading(true)
    try {
      const reservation = await api.createReservation({
        userId: user.userId,
        vehicleId: vehicle.vehicleId,
        pickUpBranchOfficeId: pickupBranch.branchOfficeId,
        dropOffBranchOfficeId: dropOffId,
        startTime: start.toISOString(),
        endTime: end.toISOString(),
      })
      onClose()
      navigate(`/checkout/${reservation.reservationId}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al crear la reserva')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card modal-card--booking" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>×</button>
        <h2>Reservar vehículo</h2>
        <img
          className="modal-vehicle-image"
          src={vehicleInfo.imageUrl}
          alt={`${vehicle.brand} ${vehicle.model}`}
        />
        <p className="modal-vehicle">{vehicle.brand} {vehicle.model} ({vehicle.year})</p>
        <p className="modal-branch">Retiro: {pickupBranch.name}</p>

        {loadingRanges ? (
          <p className="cal-loading">Cargando disponibilidad...</p>
        ) : (
          <AvailabilityCalendar
            bookedRanges={bookedRanges}
            startDate={startDate}
            endDate={endDate}
            onRangeChange={handleRangeChange}
          />
        )}

        <form onSubmit={handleSubmit} className="booking-form">
          <label>
            Sucursal de devolución
            <select value={dropOffId} onChange={(e) => setDropOffId(Number(e.target.value))}>
              {branches.map((b) => (
                <option key={b.branchOfficeId} value={b.branchOfficeId}>{b.name}</option>
              ))}
            </select>
          </label>

          <div className="date-row">
            <label>
              Hora inicio
              <input type="time" value={startTime} onChange={(e) => setStartTime(e.target.value)} required />
            </label>
            <label>
              Hora fin
              <input type="time" value={endTime} onChange={(e) => setEndTime(e.target.value)} required />
            </label>
          </div>

          {conflict && (
            <p className="modal-error">Las fechas elegidas se superponen con una reserva existente (días rojos).</p>
          )}

          {estimated > 0 && (
            <p className="modal-estimate">Costo estimado: <strong>${estimated.toLocaleString('es-AR')}</strong></p>
          )}

          {error && <p className="modal-error">{error}</p>}

          <button type="submit" disabled={loading || inMaintenance || !!conflict || loadingRanges}>
            {loading ? 'Creando reserva...' : 'Continuar al pago'}
          </button>
        </form>
      </div>
    </div>
  )
}
