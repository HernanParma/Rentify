import { useState } from 'react'
import { getVehicleCatalogEntry } from '../data/vehicleCatalog'
import type { BranchMapItem, Vehicle } from '../types'
import './VehicleDetailModal.css'

interface Props {
  vehicle: Vehicle
  branch?: BranchMapItem | null
  onClose: () => void
  onReserve?: () => void
  showReserve?: boolean
}

export default function VehicleDetailModal({
  vehicle,
  branch,
  onClose,
  onReserve,
  showReserve = true,
}: Props) {
  const info = getVehicleCatalogEntry(vehicle.brand, vehicle.model)
  const [activeIndex, setActiveIndex] = useState(0)
  const activeImage = info.images[activeIndex] ?? info.imageUrl

  return (
    <div className="vehicle-detail-overlay" onClick={onClose}>
      <div className="vehicle-detail-modal" onClick={(e) => e.stopPropagation()}>
        <button type="button" className="vehicle-detail-close" onClick={onClose} aria-label="Cerrar">
          ×
        </button>

        <div className="vehicle-detail-gallery">
          <img
            className="vehicle-detail-main-image"
            src={activeImage}
            alt={`${vehicle.brand} ${vehicle.model} — foto ${activeIndex + 1}`}
          />
          <div className="vehicle-detail-thumbs">
            {info.images.map((src, index) => (
              <button
                key={src}
                type="button"
                className={`vehicle-detail-thumb ${index === activeIndex ? 'active' : ''}`}
                onClick={() => setActiveIndex(index)}
              >
                <img src={src} alt={`Vista ${index + 1}`} />
              </button>
            ))}
          </div>
        </div>

        <div className="vehicle-detail-content">
          <header className="vehicle-detail-header">
            <div>
              <h2>{vehicle.brand} {vehicle.model}</h2>
              <p className="vehicle-detail-subtitle">Año {vehicle.year} · Patente {vehicle.plate}</p>
            </div>
            <div className="vehicle-detail-price">
              <span>Tarifa</span>
              <strong>${vehicle.pricePerDay.toLocaleString('es-AR')}/día</strong>
            </div>
          </header>

          <p className="vehicle-detail-description">{info.description}</p>

          <div className="vehicle-detail-specs">
            <div><span>Tipo</span><strong>{info.specs.category}</strong></div>
            <div><span>Transmisión</span><strong>{info.specs.transmission}</strong></div>
            <div><span>Combustible</span><strong>{info.specs.fuel}</strong></div>
            <div><span>Plazas</span><strong>{info.specs.seats}</strong></div>
            <div><span>Baúl</span><strong>{info.specs.luggage}</strong></div>
            <div><span>Seguro</span><strong>{vehicle.insurance}</strong></div>
          </div>

          {branch && (
            <p className="vehicle-detail-branch">
              Disponible en <strong>{branch.name}</strong> — {branch.address}
            </p>
          )}

          {showReserve && onReserve && vehicle.vehicleStatusName !== 'Maintenance' && (
            <button type="button" className="vehicle-detail-reserve" onClick={onReserve}>
              Reservar este vehículo
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
