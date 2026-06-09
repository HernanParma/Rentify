import VehicleCard from './VehicleCard'
import type { BranchMapItem, Vehicle } from '../types'
import './BranchVehiclesModal.css'

interface Props {
  branch: BranchMapItem
  vehicles: Vehicle[]
  loading: boolean
  periodLabel?: string
  onClose: () => void
  onVehicleDetail: (vehicle: Vehicle) => void
  onReserve: (vehicle: Vehicle) => void
}

export default function BranchVehiclesModal({
  branch,
  vehicles,
  loading,
  periodLabel,
  onClose,
  onVehicleDetail,
  onReserve,
}: Props) {
  return (
    <div className="branch-vehicles-overlay" onClick={onClose}>
      <div className="branch-vehicles-modal" onClick={(e) => e.stopPropagation()}>
        <button type="button" className="branch-vehicles-close" onClick={onClose} aria-label="Cerrar">
          ×
        </button>

        <header className="branch-vehicles-header">
          <div>
            <h2>{branch.name}</h2>
            <p className="branch-vehicles-address">{branch.address}</p>
            {periodLabel && <p className="branch-vehicles-period">{periodLabel}</p>}
          </div>
          {!loading && (
            <span className={`branch-vehicles-count ${vehicles.length > 0 ? 'branch-vehicles-count--ok' : 'branch-vehicles-count--none'}`}>
              {vehicles.length > 0
                ? `${vehicles.length} disponible${vehicles.length === 1 ? '' : 's'}`
                : 'Sin stock'}
            </span>
          )}
        </header>

        <div className="branch-vehicles-body">
          {loading ? (
            <p className="branch-vehicles-empty">Buscando autos en esta sucursal...</p>
          ) : vehicles.length === 0 ? (
            <p className="branch-vehicles-empty">
              No hay vehículos en esta sucursal para esas fechas. Probá otra sede o cambiá las fechas.
            </p>
          ) : (
            <div className="branch-vehicles-grid">
              {vehicles.map((vehicle) => (
                <VehicleCard key={vehicle.vehicleId} vehicle={vehicle} onClick={() => onVehicleDetail(vehicle)}>
                  <button
                    type="button"
                    className="branch-vehicles-reserve-btn"
                    onClick={(e) => {
                      e.stopPropagation()
                      onReserve(vehicle)
                    }}
                  >
                    Reservar
                  </button>
                </VehicleCard>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
