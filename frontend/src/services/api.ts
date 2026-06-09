import type {
  AdminUser,
  BookedRange,
  Branch,
  BranchMapItem,
  NotificationRecord,
  PaymentResponse,
  Reservation,
  ReservationFilters,
  Vehicle,
} from '../types'

const API_BASE = import.meta.env.VITE_API_URL || ''

function getToken(): string | null {
  return localStorage.getItem('rentify_token')
}

function buildQuery(params: Record<string, string | number | undefined>): string {
  const qs = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== '') qs.set(key, String(value))
  }
  const str = qs.toString()
  return str ? `?${str}` : ''
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken()
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  }
  if (token) headers['Authorization'] = `Bearer ${token}`

  let response: Response
  try {
    response = await fetch(`${API_BASE}${path}`, { ...options, headers })
  } catch {
    throw new Error(
      'No se pudo conectar al servidor. Verificá que ApiGateway esté corriendo (puerto 5000) y los microservicios activos.',
    )
  }

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Sesión expirada o no autorizada. Cerrá sesión e iniciá de nuevo.')
    }
    if (response.status === 502 || response.status === 503) {
      let msg = 'Un microservicio no responde. Ejecutá start-dev.ps1.'
      if (path.includes('/api/Payment')) msg = 'PaymentMS no responde (puerto 5099). Reiniciá con start-dev.ps1.'
      else if (path.includes('/api/v1/Reservations')) msg = 'ReservationMS no responde (puerto 5055). Reiniciá con start-dev.ps1.'
      else if (path.includes('/api/v1/Vehicles')) msg = 'VehicleMS no responde (puerto 5054). Reiniciá con start-dev.ps1.'
      else if (path.includes('/api/v1/BranchOffices')) msg = 'BranchOfficeMS no responde (puerto 5053). Reiniciá con start-dev.ps1.'
      else if (path.includes('/api/v1/Auth')) msg = 'AuthMS no responde (puerto 5093). Reiniciá con start-dev.ps1.'
      throw new Error(msg)
    }
    const error = await response.json().catch(() => ({ message: `Error ${response.status}` }))
    throw new Error(error.message || error.Message || `Error ${response.status}`)
  }
  if (response.status === 204) return undefined as T
  return response.json()
}

export const api = {
  login: (email: string, password: string) =>
    request<{ accessToken: string; refreshToken: string; result: boolean; message: string }>(
      '/api/v1/Auth/Login',
      { method: 'POST', body: JSON.stringify({ email, password }) },
    ),

  register: (data: { firstName: string; lastName: string; email: string; dni: string; password: string }) =>
    request('/api/v1/User', { method: 'POST', body: JSON.stringify(data) }),

  getBranchesMap: () => request<BranchMapItem[]>('/api/v1/BranchOffices/map'),
  getBranches: () => request<Branch[]>('/api/v1/BranchOffices'),

  createBranch: (data: Omit<Branch, 'branchOfficeId'>) =>
    request<Branch>('/api/v1/BranchOffices', { method: 'POST', body: JSON.stringify(data) }),

  updateBranch: (id: number, data: Omit<Branch, 'branchOfficeId'>) =>
    request<Branch>(`/api/v1/BranchOffices/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  deleteBranch: (id: number) =>
    request<void>(`/api/v1/BranchOffices/${id}`, { method: 'DELETE' }),

  getVehiclesByBranch: (branchId: number) =>
    request<Vehicle[]>(`/api/v1/Vehicles/branch/${branchId}`),

  getAllVehicles: () => request<Vehicle[]>('/api/v1/Vehicles'),

  createVehicle: (data: {
    brand: string
    model: string
    year: number
    plate: string
    vehicleStatusId: number
    pricePerDay: number
    branchOfficeId: number
    insurance: string
  }) => request<Vehicle>('/api/v1/Vehicles', { method: 'POST', body: JSON.stringify(data) }),

  updateVehicle: (id: string, data: {
    brand: string
    model: string
    year: number
    plate: string
    vehicleStatusId: number
    pricePerDay: number
    branchOfficeId: number
    insurance: string
  }) => request<Vehicle>(`/api/v1/Vehicles/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  deleteVehicle: (id: string) =>
    request<void>(`/api/v1/Vehicles/${id}`, { method: 'DELETE' }),

  createReservation: (data: {
    userId: number
    vehicleId: string
    pickUpBranchOfficeId: number
    dropOffBranchOfficeId: number
    startTime: string
    endTime: string
  }) => request<Reservation>('/api/v1/Reservations', { method: 'POST', body: JSON.stringify(data) }),

  getReservation: (id: string) => request<Reservation>(`/api/v1/Reservations/${id}`),
  getUserReservations: (userId: number) =>
    request<Reservation[]>(`/api/v1/Reservations/user/${userId}`),

  getVehicleBookedRanges: (vehicleId: string) =>
    request<BookedRange[]>(`/api/v1/Reservations/vehicle/${vehicleId}/booked-ranges`),

  getAvailableVehicles: (branchId: number, startTime: string, endTime: string) =>
    request<Vehicle[]>(
      `/api/v1/Reservations/availability${buildQuery({
        branchId,
        start: startTime,
        end: endTime,
      })}`,
    ),

  cancelReservation: (reservationId: string) =>
    request<Reservation>(`/api/v1/Reservations/${reservationId}/cancel`, { method: 'POST' }),

  getAllReservations: (filters: ReservationFilters = {}) =>
    request<Reservation[]>(`/api/v1/Reservations${buildQuery({
      statusId: filters.statusId,
      branchId: filters.branchId,
      userId: filters.userId,
      search: filters.search,
      from: filters.from,
      to: filters.to,
    })}`),

  registerPickup: (reservationId: string, timestamp?: string) =>
    request<Reservation>(`/api/v1/Reservations/${reservationId}/pickup`, {
      method: 'POST',
      body: JSON.stringify({ timestamp: timestamp ? new Date(timestamp).toISOString() : null }),
    }),

  registerReturn: (reservationId: string, timestamp?: string) =>
    request<Reservation>(`/api/v1/Reservations/${reservationId}/return`, {
      method: 'POST',
      body: JSON.stringify({ timestamp: timestamp ? new Date(timestamp).toISOString() : null }),
    }),

  getAdminUsers: () => request<AdminUser[]>('/api/v1/Admin/users'),

  updateUserRole: (userId: number, role: string) =>
    request<AdminUser>(`/api/v1/Admin/users/${userId}/role`, {
      method: 'PATCH',
      body: JSON.stringify({ role }),
    }),

  updateUserStatus: (userId: number, isActive: boolean) =>
    request<AdminUser>(`/api/v1/Admin/users/${userId}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ isActive }),
    }),

  getNotificationHistory: (params: { userId?: number; status?: string; limit?: number } = {}) =>
    request<NotificationRecord[]>(`/api/v1/Admin/notifications${buildQuery({
      userId: params.userId,
      status: params.status,
      limit: params.limit,
    })}`),

  createPayment: (reservation: {
    reservationId: string
    userId: number
    startTime: string
    endTime: string
    actualPickupTime?: string | null
    actualReturnTime?: string | null
    hourlyRateSnapshot: number
  }) =>
    request<PaymentResponse>('/api/Payment/from-reservation', {
      method: 'POST',
      body: JSON.stringify({
        reservationId: reservation.reservationId,
        userId: reservation.userId,
        startTime: reservation.startTime,
        endTime: reservation.endTime,
        actualPickupTime: reservation.actualPickupTime ?? null,
        actualReturnTime: reservation.actualReturnTime ?? null,
        hourlyRateSnapshot: reservation.hourlyRateSnapshot,
      }),
    }),

  verifyPayment: (mercadoPagoPaymentId: number) =>
    request<Record<string, unknown>>(`/api/Payment/verify/${mercadoPagoPaymentId}`, { method: 'POST' }),

  completeMockPayment: (localPaymentId: string) =>
    request<{ reservationId: string; amount: number; status: string }>(
      `/api/Payment/mock-complete/${localPaymentId}`,
      { method: 'POST' },
    ),
}

export function estimateCost(pricePerDay: number, start: Date, end: Date): number {
  const hours = Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60))
  const hourlyRate = pricePerDay / 24
  return Math.round(hourlyRate * hours * 100) / 100
}
