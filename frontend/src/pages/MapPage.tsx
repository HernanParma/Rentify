import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import MapInvalidateSize from '../components/MapInvalidateSize'
import { api } from '../services/api'
import { getStoredUser } from '../utils/jwt'
import BookingModal from '../components/BookingModal'
import BranchesModal from '../components/BranchesModal'
import BranchVehiclesModal from '../components/BranchVehiclesModal'
import ReservationDatesModal from '../components/ReservationDatesModal'
import VehicleDetailModal from '../components/VehicleDetailModal'
import type { BranchMapItem, Vehicle } from '../types'
import './MapPage.css'

const MARKER_SHADOW = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png'
const MARKER_BASE = 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img'

function createMarkerIcon(color: 'blue' | 'green' | 'red' | 'grey') {
  return new L.Icon({
    iconUrl: `${MARKER_BASE}/marker-icon-2x-${color}.png`,
    shadowUrl: MARKER_SHADOW,
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41],
  })
}

const markerIconDefault = createMarkerIcon('blue')
const markerIconGreen = createMarkerIcon('green')
const markerIconRed = createMarkerIcon('red')
const markerIconGrey = createMarkerIcon('grey')

function localDateInputValue(d = new Date()) {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

export default function MapPage() {
  const navigate = useNavigate()
  const user = getStoredUser()
  const [branches, setBranches] = useState<BranchMapItem[]>([])
  const [selectedBranch, setSelectedBranch] = useState<BranchMapItem | null>(null)
  const [branchModalBranch, setBranchModalBranch] = useState<BranchMapItem | null>(null)
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [bookingVehicle, setBookingVehicle] = useState<Vehicle | null>(null)
  const [detailVehicle, setDetailVehicle] = useState<Vehicle | null>(null)
  const [loading, setLoading] = useState(true)
  const [loadingVehicles, setLoadingVehicles] = useState(false)
  const [error, setError] = useState('')

  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [startTime, setStartTime] = useState('10:00')
  const [endTime, setEndTime] = useState('18:00')
  const [datesReady, setDatesReady] = useState(false)
  const [branchStock, setBranchStock] = useState<Record<number, number>>({})
  const [loadingBranchStock, setLoadingBranchStock] = useState(false)

  const [datesModalOpen, setDatesModalOpen] = useState(false)
  const [branchesModalOpen, setBranchesModalOpen] = useState(false)
  const [mapReady, setMapReady] = useState(false)

  useEffect(() => {
    if (!localStorage.getItem('rentify_token')) {
      navigate('/')
      return
    }

    const tomorrow = new Date()
    tomorrow.setDate(tomorrow.getDate() + 1)
    const dayAfter = new Date()
    dayAfter.setDate(dayAfter.getDate() + 3)
    setStartDate(localDateInputValue(tomorrow))
    setEndDate(localDateInputValue(dayAfter))

    api.getBranchesMap()
      .then(setBranches)
      .catch((err) => setError(err.message))
      .finally(() => {
        setLoading(false)
        setDatesModalOpen(true)
      })
  }, [navigate])

  useEffect(() => {
    if (loading) {
      setMapReady(false)
      return
    }

    const frame = requestAnimationFrame(() => {
      setMapReady(true)
    })
    return () => cancelAnimationFrame(frame)
  }, [loading])

  const mapResizeTrigger = [
    mapReady,
    datesModalOpen,
    branchesModalOpen,
    branchModalBranch?.branchOfficeId,
    !!error,
  ].join('|')

  const start = startDate && startTime ? new Date(`${startDate}T${startTime}`) : null
  const end = endDate && endTime ? new Date(`${endDate}T${endTime}`) : null
  const datesValid = !!(start && end && end > start && start >= new Date())

  const refreshBranchAvailability = async () => {
    if (!datesValid || !start || !end || branches.length === 0) return

    setLoadingBranchStock(true)
    setBranchStock({})
    try {
      const results = await Promise.all(
        branches.map(async (branch) => {
          const data = await api.getAvailableVehicles(
            branch.branchOfficeId,
            start.toISOString(),
            end.toISOString(),
          )
          return { branchOfficeId: branch.branchOfficeId, count: data.length }
        }),
      )
      const stock: Record<number, number> = {}
      for (const item of results) stock[item.branchOfficeId] = item.count
      setBranchStock(stock)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al consultar disponibilidad por sucursal')
    } finally {
      setLoadingBranchStock(false)
    }
  }

  const getBranchMarkerIcon = (branchOfficeId: number) => {
    if (!datesReady) return markerIconDefault
    if (loadingBranchStock) return markerIconGrey
    const count = branchStock[branchOfficeId]
    if (count === undefined) return markerIconGrey
    return count > 0 ? markerIconGreen : markerIconRed
  }

  const openBranchVehiclesModal = async (branch: BranchMapItem) => {
    if (!datesReady || !datesValid || !start || !end) {
      setError('Confirmá las fechas antes de elegir una sucursal.')
      setDatesModalOpen(true)
      return
    }

    setSelectedBranch(branch)
    setBranchModalBranch(branch)
    setBranchesModalOpen(false)
    setLoadingVehicles(true)
    setVehicles([])
    setError('')
    try {
      const data = await api.getAvailableVehicles(
        branch.branchOfficeId,
        start.toISOString(),
        end.toISOString(),
      )
      setVehicles(data)
    } catch (err) {
      setVehicles([])
      setError(err instanceof Error ? err.message : 'Error al buscar vehículos')
      setBranchModalBranch(null)
    } finally {
      setLoadingVehicles(false)
    }
  }

  const closeBranchVehiclesModal = () => {
    setBranchModalBranch(null)
  }

  const handleConfirmDates = async () => {
    if (!datesValid) {
      setError('Las fechas deben ser futuras y la devolución posterior al retiro.')
      return
    }
    setDatesReady(true)
    setError('')
    setDatesModalOpen(false)
    await refreshBranchAvailability()
    if (branchModalBranch) openBranchVehiclesModal(branchModalBranch)
  }

  const invalidateDates = () => {
    setDatesReady(false)
    setBranchStock({})
    setVehicles([])
    setBranchModalBranch(null)
  }

  const handleRangeChange = (startKey: string, endKey: string) => {
    setStartDate(startKey)
    setEndDate(endKey)
    invalidateDates()
  }

  const handleLogout = () => {
    localStorage.removeItem('rentify_token')
    localStorage.removeItem('rentify_refresh')
    localStorage.removeItem('rentify_user')
    navigate('/')
  }

  if (loading) return <div className="map-loading">Cargando sedes...</div>

  return (
    <div className="map-page">
      <header className="map-header">
        <div>
          <h1>Rentify</h1>
          <span>{user ? `Hola, ${user.firstName}` : 'Alquiler de autos'}</span>
        </div>
        <div className="map-header-actions">
          <button type="button" className="header-btn header-btn--primary" onClick={() => setDatesModalOpen(true)}>
            Reservar
          </button>
          <button type="button" className="header-btn" onClick={() => setBranchesModalOpen(true)}>
            Sucursales
          </button>
          <Link to="/reservas" className="header-link">Mis reservas</Link>
          <button type="button" className="header-btn" onClick={handleLogout}>Cerrar sesión</button>
        </div>
      </header>

      {error && <div className="map-error-banner">{error}</div>}

      <div className="map-layout">
        <div className="map-container">
          {mapReady && (
          <MapContainer center={[-34.6037, -58.3816]} zoom={12} className="leaflet-map" doubleClickZoom={false}>
            <MapInvalidateSize trigger={mapResizeTrigger} />
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
            {branches.map((branch) => {
              const stock = branchStock[branch.branchOfficeId]
              return (
                <Marker
                  key={`${branch.branchOfficeId}-${datesReady}-${loadingBranchStock}-${stock ?? 'pending'}`}
                  position={[branch.latitude, branch.longitude]}
                  icon={getBranchMarkerIcon(branch.branchOfficeId)}
                  eventHandlers={{
                    dblclick: (e) => {
                      L.DomEvent.stopPropagation(e)
                      if (datesReady && !loadingBranchStock) openBranchVehiclesModal(branch)
                    },
                  }}
                >
                  <Popup>
                    <strong>{branch.name}</strong>
                    <br />
                    {branch.address}
                    {datesReady && stock !== undefined && (
                      <>
                        <br />
                        <span style={{ color: stock > 0 ? '#16a34a' : '#dc2626', fontWeight: 600 }}>
                          {stock > 0 ? `${stock} auto${stock === 1 ? '' : 's'} disponible${stock === 1 ? '' : 's'}` : 'Sin autos para esas fechas'}
                        </span>
                        <br />
                        <em style={{ fontSize: '0.8rem', color: '#64748b' }}>Doble clic para ver autos</em>
                      </>
                    )}
                  </Popup>
                </Marker>
              )
            })}
          </MapContainer>
          )}
        </div>
      </div>

      {datesModalOpen && (
        <ReservationDatesModal
          startDate={startDate}
          endDate={endDate}
          startTime={startTime}
          endTime={endTime}
          datesValid={datesValid}
          onRangeChange={handleRangeChange}
          onStartTimeChange={(time) => { setStartTime(time); invalidateDates() }}
          onEndTimeChange={(time) => { setEndTime(time); invalidateDates() }}
          onConfirm={handleConfirmDates}
          onClose={() => setDatesModalOpen(false)}
        />
      )}

      {branchesModalOpen && (
        <BranchesModal
          branches={branches}
          branchStock={branchStock}
          datesReady={datesReady}
          loadingBranchStock={loadingBranchStock}
          selectedBranchId={selectedBranch?.branchOfficeId}
          onSelectBranch={openBranchVehiclesModal}
          onClose={() => setBranchesModalOpen(false)}
        />
      )}

      {branchModalBranch && (
        <BranchVehiclesModal
          branch={branchModalBranch}
          vehicles={vehicles}
          loading={loadingVehicles}
          periodLabel={
            datesReady && start && end
              ? `Del ${start.toLocaleDateString('es-AR')} ${startTime} al ${end.toLocaleDateString('es-AR')} ${endTime}`
              : undefined
          }
          onClose={closeBranchVehiclesModal}
          onVehicleDetail={setDetailVehicle}
          onReserve={(vehicle) => {
            setBookingVehicle(vehicle)
            closeBranchVehiclesModal()
          }}
        />
      )}

      {detailVehicle && (
        <VehicleDetailModal
          vehicle={detailVehicle}
          branch={selectedBranch}
          onClose={() => setDetailVehicle(null)}
          onReserve={() => {
            setBookingVehicle(detailVehicle)
            setDetailVehicle(null)
          }}
        />
      )}

      {bookingVehicle && selectedBranch && start && end && (
        <BookingModal
          vehicle={bookingVehicle}
          pickupBranch={selectedBranch}
          initialStartDate={startDate}
          initialEndDate={endDate}
          initialStartTime={startTime}
          initialEndTime={endTime}
          onClose={() => setBookingVehicle(null)}
        />
      )}
    </div>
  )
}
