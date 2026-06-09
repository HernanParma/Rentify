export interface JwtUser {
  userId: number
  firstName: string
  lastName: string
  email: string
  role: string
}

export function parseJwt(token: string): JwtUser | null {
  try {
    const payload = token.split('.')[1]
    const decoded = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')))
    const userId = parseInt(decoded.UserId || decoded.sub || decoded.nameid, 10)
    if (!userId) return null
    return {
      userId,
      firstName: decoded.FirstName || '',
      lastName: decoded.LastName || '',
      email: decoded.UserEmail || decoded.email || '',
      role: decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.UserRole || 'Customer',
    }
  } catch {
    return null
  }
}

export function getStoredUser(): JwtUser | null {
  const token = localStorage.getItem('rentify_token')
  if (!token) return null
  return parseJwt(token)
}

export function saveUserFromToken(token: string): JwtUser | null {
  const user = parseJwt(token)
  if (user) {
    localStorage.setItem('rentify_user', JSON.stringify(user))
  }
  return user
}
