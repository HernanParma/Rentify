import type { BranchMapItem } from '../types'
import './BranchesModal.css'

interface Props {
  branches: BranchMapItem[]
  branchStock: Record<number, number>
  datesReady: boolean
  loadingBranchStock: boolean
  selectedBranchId?: number
  onSelectBranch: (branch: BranchMapItem) => void
  onClose: () => void
}

export default function BranchesModal({
  branches,
  branchStock,
  datesReady,
  loadingBranchStock,
  selectedBranchId,
  onSelectBranch,
  onClose,
}: Props) {
  return (
    <div className="branches-overlay" onClick={onClose}>
      <div className="branches-modal" onClick={(e) => e.stopPropagation()}>
        <button type="button" className="branches-close" onClick={onClose} aria-label="Cerrar">
          ×
        </button>

        <header className="branches-header">
          <h2>Sucursales</h2>
          {!datesReady && (
            <p className="branches-hint">Confirmá fechas con el botón Reservar para ver stock disponible.</p>
          )}
          {datesReady && loadingBranchStock && (
            <p className="branches-hint branches-hint--info">Consultando stock en cada sucursal...</p>
          )}
          {datesReady && !loadingBranchStock && (
            <p className="branches-hint branches-hint--info">Doble clic en un pin del mapa para ver los autos.</p>
          )}
        </header>

        <div className="branches-list">
          {branches.map((branch) => {
            const stock = branchStock[branch.branchOfficeId]
            const hasStock = stock !== undefined && stock > 0
            const noStock = stock !== undefined && stock === 0
            return (
              <button
                key={branch.branchOfficeId}
                type="button"
                className={[
                  'branches-card',
                  selectedBranchId === branch.branchOfficeId ? 'active' : '',
                  hasStock ? 'branches-card--stock' : '',
                  noStock ? 'branches-card--empty' : '',
                ].filter(Boolean).join(' ')}
                onClick={() => onSelectBranch(branch)}
                disabled={!datesReady || loadingBranchStock}
              >
                <strong>{branch.name}</strong>
                <span>{branch.address}</span>
                <span className="branches-hours">{branch.hours}</span>
                {datesReady && stock !== undefined && (
                  <span className={`branches-stock ${hasStock ? 'branches-stock--ok' : 'branches-stock--none'}`}>
                    {hasStock
                      ? `${stock} auto${stock === 1 ? '' : 's'} disponible${stock === 1 ? '' : 's'}`
                      : 'Sin autos para esas fechas'}
                  </span>
                )}
              </button>
            )
          })}
        </div>
      </div>
    </div>
  )
}
