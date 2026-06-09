import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { Branch } from '../types'

const emptyForm = {
  name: '',
  address: '',
  phone: '',
  hours: 'Lun-Dom 08:00-20:00',
  latitude: -34.6037,
  longitude: -58.3816,
  isActive: true,
}

export default function AdminBranchesPage() {
  const [branches, setBranches] = useState<Branch[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState(emptyForm)
  const [saving, setSaving] = useState(false)

  const load = () => {
    setLoading(true)
    api.getBranches()
      .then(setBranches)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const openCreate = () => {
    setEditingId(null)
    setForm(emptyForm)
    setShowForm(true)
  }

  const openEdit = (b: Branch) => {
    setEditingId(b.branchOfficeId)
    setForm({
      name: b.name,
      address: b.address,
      phone: b.phone,
      hours: b.hours,
      latitude: b.latitude,
      longitude: b.longitude,
      isActive: b.isActive,
    })
    setShowForm(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError('')
    try {
      if (editingId) {
        const updated = await api.updateBranch(editingId, form)
        setBranches((prev) => prev.map((b) => b.branchOfficeId === editingId ? updated : b))
      } else {
        const created = await api.createBranch(form)
        setBranches((prev) => [...prev, created])
      }
      setShowForm(false)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al guardar')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (id: number) => {
    if (!confirm('¿Eliminar esta sucursal?')) return
    try {
      await api.deleteBranch(id)
      setBranches((prev) => prev.filter((b) => b.branchOfficeId !== id))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al eliminar')
    }
  }

  if (loading) return <p className="admin-loading">Cargando sucursales...</p>

  return (
    <>
      <div className="admin-page-header">
        <h1>Sucursales</h1>
        <button className="admin-btn admin-btn-primary" onClick={openCreate}>+ Nueva sucursal</button>
      </div>
      {error && <p className="admin-error">{error}</p>}

      {showForm && (
        <div className="admin-form-panel">
          <h3>{editingId ? 'Editar sucursal' : 'Nueva sucursal'}</h3>
          <div className="admin-form-grid">
            <input placeholder="Nombre" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            <input placeholder="Dirección" value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
            <input placeholder="Teléfono" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
            <input placeholder="Horarios" value={form.hours} onChange={(e) => setForm({ ...form, hours: e.target.value })} />
            <input type="number" step="0.0001" placeholder="Latitud" value={form.latitude} onChange={(e) => setForm({ ...form, latitude: +e.target.value })} />
            <input type="number" step="0.0001" placeholder="Longitud" value={form.longitude} onChange={(e) => setForm({ ...form, longitude: +e.target.value })} />
            <label className="admin-checkbox">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
              Activa
            </label>
          </div>
          <div className="admin-form-actions">
            <button className="admin-btn admin-btn-primary" onClick={handleSave} disabled={saving}>
              {saving ? 'Guardando...' : 'Guardar'}
            </button>
            <button className="admin-btn admin-btn-secondary" onClick={() => setShowForm(false)}>Cancelar</button>
          </div>
        </div>
      )}

      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Dirección</th>
              <th>Teléfono</th>
              <th>Horarios</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {branches.map((b) => (
              <tr key={b.branchOfficeId}>
                <td>{b.name}</td>
                <td>{b.address}</td>
                <td>{b.phone}</td>
                <td>{b.hours}</td>
                <td>
                  <span className="admin-badge" style={{ background: b.isActive ? '#10b981' : '#94a3b8' }}>
                    {b.isActive ? 'Activa' : 'Inactiva'}
                  </span>
                </td>
                <td>
                  <div className="admin-inline-actions">
                    <button className="admin-btn admin-btn-secondary" onClick={() => openEdit(b)}>Editar</button>
                    <button className="admin-btn admin-btn-danger" onClick={() => handleDelete(b.branchOfficeId)}>Eliminar</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  )
}
