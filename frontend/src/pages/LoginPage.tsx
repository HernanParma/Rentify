import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../services/api'
import { saveUserFromToken } from '../utils/jwt'
import './LoginPage.css'

export default function LoginPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [dni, setDni] = useState('')
  const [isRegister, setIsRegister] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      if (isRegister) {
        await api.register({ firstName, lastName, email, dni, password })
        setError('Registro exitoso. Revisá tu email para verificar la cuenta.')
        setIsRegister(false)
      } else {
        const result = await api.login(email, password)
        localStorage.setItem('rentify_token', result.accessToken)
        localStorage.setItem('rentify_refresh', result.refreshToken)
        const user = saveUserFromToken(result.accessToken)
        if (user?.role === 'Admin') navigate('/admin')
        else if (user?.role === 'Employee') navigate('/admin/reservas')
        else navigate('/mapa')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error desconocido')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-brand">
          <h1>Rentify</h1>
          <p>Alquilá autos en las mejores sedes de la ciudad</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          {isRegister && (
            <>
              <input placeholder="Nombre" value={firstName} onChange={(e) => setFirstName(e.target.value)} required />
              <input placeholder="Apellido" value={lastName} onChange={(e) => setLastName(e.target.value)} required />
              <input placeholder="DNI" value={dni} onChange={(e) => setDni(e.target.value)} required />
            </>
          )}
          <input type="email" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          <input type="password" placeholder="Contraseña" value={password} onChange={(e) => setPassword(e.target.value)} required />

          {error && <p className="login-error">{error}</p>}

          <button type="submit" disabled={loading}>
            {loading ? 'Cargando...' : isRegister ? 'Registrarse' : 'Iniciar sesión'}
          </button>
        </form>

        <button className="login-toggle" onClick={() => setIsRegister(!isRegister)}>
          {isRegister ? '¿Ya tenés cuenta? Iniciá sesión' : '¿No tenés cuenta? Registrate'}
        </button>

        {!isRegister && (
          <p className="login-demo">
            Demo cliente: demo@rentify.com / Demo123!<br />
            Admin: admin@rentify.com / Demo123!
          </p>
        )}
      </div>
    </div>
  )
}
