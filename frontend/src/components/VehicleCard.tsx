import type { ReactNode } from 'react'
import { getVehicleCatalogEntry } from '../data/vehicleCatalog'
import type { Vehicle } from '../types'
import './VehicleCard.css'

interface Props {
  vehicle: Vehicle
  children?: ReactNode
  compact?: boolean
  onClick?: () => void
}

export default function VehicleCard({ vehicle, children, compact = false, onClick }: Props) {
  const { imageUrl, description } = getVehicleCatalogEntry(vehicle.brand, vehicle.model)

  return (
    <article
      className={`vehicle-card ${compact ? 'vehicle-card--compact' : ''} ${onClick ? 'vehicle-card--clickable' : ''}`}
      onClick={onClick}
      onKeyDown={onClick ? (e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onClick() } } : undefined}
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
    >
      <img
        className="vehicle-card-image"
        src={imageUrl}
        alt={`${vehicle.brand} ${vehicle.model}`}
        loading="lazy"
      />
      <div className="vehicle-card-body">
        <strong>{vehicle.brand} {vehicle.model} ({vehicle.year})</strong>
        {vehicle.vehicleStatusName === 'Maintenance' && (
          <span className="vehicle-card-status vehicle-card-status--maintenance">En mantenimiento</span>
        )}
        {vehicle.vehicleStatusName === 'Rented' && (
          <span className="vehicle-card-status vehicle-card-status--rented">Reservable por fechas</span>
        )}
        <p className="vehicle-card-description">{description}</p>
        {!compact && (
          <>
            <span>Patente: {vehicle.plate}</span>
            <span>${vehicle.pricePerDay.toLocaleString('es-AR')}/día</span>
            <span>Seguro: {vehicle.insurance}</span>
          </>
        )}
        {onClick && <span className="vehicle-card-hint">Ver detalle y fotos →</span>}
        {children}
      </div>
    </article>
  )
}
