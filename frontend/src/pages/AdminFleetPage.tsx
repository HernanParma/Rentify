import { useEffect, useState } from 'react'
import { api } from '../services/api'
import { getVehicleCatalogEntry } from '../data/vehicleCatalog'
import VehicleDetailModal from '../components/VehicleDetailModal'
import type { Branch, Vehicle } from '../types'

const STATUS_OPTIONS = [
  { id: 1, name: 'Available' },
  { id: 2, name: 'Rented' },
  { id: 3, name: 'Maintenance' },
]

const statusColors: Record<string, string> = {
  Available: '#10b981',
  Rented: '#3b82f6',
  Maintenance: '#f59e0b',
}

const emptyForm = {
  brand: '',
  model: '',
  year: new Date().getFullYear(),
  plate: '',
  vehicleStatusId: 1,
  pricePerDay: 40000,
  branchOfficeId: 1,
  insurance: 'Allianz Full',
}

export default function AdminFleetPage() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [branches, setBranches] = useState<Branch[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState(emptyForm)
  const [saving, setSaving] = useState(false)
  const [detailVehicle, setDetailVehicle] = useState<Vehicle | null>(null)

  const load = () => {
    setLoading(true)
    Promise.all([api.getAllVehicles(), api.getBranches()])
      .then(([v, b]) => { setVehicles(v); setBranches(b) })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const openCreate = () => {
    setEditingId(null)
    setForm({ ...emptyForm, branchOfficeId: branches[0]?.branchOfficeId ?? 1 })
    setShowForm(true)
  }

  const openEdit = (v: Vehicle) => {
    setEditingId(v.vehicleId)
    setForm({
      brand: v.brand,
      model: v.model,
      year: v.year,
      plate: v.plate,
      vehicleStatusId: STATUS_OPTIONS.find((s) => s.name === v.vehicleStatusName)?.id ?? 1,
      pricePerDay: v.pricePerDay,
      branchOfficeId: v.branchOfficeId,
      insurance: v.insurance,
    })
    setShowForm(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError('')
    try {
      if (editingId) {
        const updated = await api.updateVehicle(editingId, form)
        setVehicles((prev) => prev.map((v) => v.vehicleId === editingId ? updated : v))
      } else {
        const created = await api.createVehicle(form)
        setVehicles((prev) => [...prev, created])
      }
      setShowForm(false)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al guardar')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm('¿Eliminar este vehículo?')) return
    try {
      await api.deleteVehicle(id)
      setVehicles((prev) => prev.filter((v) => v.vehicleId !== id))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al eliminar')
    }
  }

  if (loading) return <p className="admin-loading">Cargando flota...</p>

  return (
    <>
      <div className="admin-page-header">
        <h1>Flota de vehículos</h1>
        <button className="admin-btn admin-btn-primary" onClick={openCreate}>+ Nuevo vehículo</button>
      </div>
      {error && <p className="admin-error">{error}</p>}

      {showForm && (
        <div className="admin-form-panel">
          <h3>{editingId ? 'Editar vehículo' : 'Nuevo vehículo'}</h3>
          <div className="admin-form-grid">
            <input placeholder="Marca" value={form.brand} onChange={(e) => setForm({ ...form, brand: e.target.value })} />
            <input placeholder="Modelo" value={form.model} onChange={(e) => setForm({ ...form, model: e.target.value })} />
            <input type="number" placeholder="Año" value={form.year} onChange={(e) => setForm({ ...form, year: +e.target.value })} />
            <input placeholder="Patente" value={form.plate} onChange={(e) => setForm({ ...form, plate: e.target.value })} />
            <input type="number" placeholder="Precio/día" value={form.pricePerDay} onChange={(e) => setForm({ ...form, pricePerDay: +e.target.value })} />
            <input placeholder="Seguro" value={form.insurance} onChange={(e) => setForm({ ...form, insurance: e.target.value })} />
            <select value={form.branchOfficeId} onChange={(e) => setForm({ ...form, branchOfficeId: +e.target.value })}>
              {branches.map((b) => <option key={b.branchOfficeId} value={b.branchOfficeId}>{b.name}</option>)}
            </select>
            <select value={form.vehicleStatusId} onChange={(e) => setForm({ ...form, vehicleStatusId: +e.target.value })}>
              {STATUS_OPTIONS.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
          </div>
          <div className="admin-form-actions">
            <button className="admin-btn admin-btn-primary" onClick={handleSave} disabled={saving}>
              {saving ? 'Guardando...' : 'Guardar'}
            </button>
            <button className="admin-btn admin-btn-secondary" onClick={() => setShowForm(false)}>Cancelar</button>
          </div>
        </div>
      )}

      <div className="admin-fleet-grid">
        {vehicles.map((v) => {
          const info = getVehicleCatalogEntry(v.brand, v.model)
          return (
          <div
            key={v.vehicleId}
            className="admin-fleet-card admin-fleet-card--clickable"
            onClick={() => setDetailVehicle(v)}
            onKeyDown={(e) => { if (e.key === 'Enter') setDetailVehicle(v) }}
            role="button"
            tabIndex={0}
          >
            <img className="admin-fleet-image" src={info.imageUrl} alt={`${v.brand} ${v.model}`} />
            <h3>{v.brand} {v.model} ({v.year})</h3>
            <p className="admin-fleet-description">{info.description}</p>
            <p><strong>Patente:</strong> {v.plate}</p>
            <p><strong>Sede:</strong> {branches.find((b) => b.branchOfficeId === v.branchOfficeId)?.name ?? `#${v.branchOfficeId}`}</p>
            <p><strong>Tarifa:</strong> ${v.pricePerDay.toLocaleString('es-AR')}/día</p>
            <p><strong>Seguro:</strong> {v.insurance}</p>
            <span className="admin-badge" style={{ background: statusColors[v.vehicleStatusName] || '#94a3b8' }}>
              {v.vehicleStatusName}
            </span>
            <div className="admin-card-actions" onClick={(e) => e.stopPropagation()}>
              <button className="admin-btn admin-btn-secondary" onClick={() => openEdit(v)}>Editar</button>
              <button className="admin-btn admin-btn-danger" onClick={() => handleDelete(v.vehicleId)}>Eliminar</button>
            </div>
          </div>
        )})}
      </div>

      {detailVehicle && (
        <VehicleDetailModal
          vehicle={detailVehicle}
          onClose={() => setDetailVehicle(null)}
          showReserve={false}
        />
      )}
    </>
  )
}