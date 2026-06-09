export interface BranchMapItem {
  branchOfficeId: number
  name: string
  address: string
  phone: string
  hours: string
  latitude: number
  longitude: number
  availableVehicleCount: number
}

export interface Branch {
  branchOfficeId: number
  name: string
  address: string
  phone: string
  hours: string
  latitude: number
  longitude: number
  isActive: boolean
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  result: boolean
  message: string
}

export interface Vehicle {
  vehicleId: string
  brand: string
  model: string
  year: number
  plate: string
  pricePerDay: number
  branchOfficeId: number
  vehicleStatusId?: number
  vehicleStatusName: string
  insurance: string
}

export interface AdminUser {
  userId: number
  firstName: string
  lastName: string
  email: string
  dni: string
  role: string
  isActive: boolean
  isEmailVerified: boolean
}

export interface NotificationRecord {
  notificationId: string
  userId: number
  userEmail: string
  type: string
  status: string
  createdAt: string
  sentAt?: string
}

export interface ReservationFilters {
  statusId?: number
  branchId?: number
  userId?: number
  search?: string
  from?: string
  to?: string
}

export interface Reservation {
  reservationId: string
  userId: number
  vehicleId: string
  reservationStatusId: number
  reservationStatusName: string
  pickupBranchOfficeId: number
  pickupBranchOfficeName: string
  dropOffBranchOfficeId: number
  dropOffBranchOfficeName: string
  startTime: string
  endTime: string
  actualPickupTime?: string
  actualReturnTime?: string
  hourlyRateSnapshot: number
  totalCost: number
}

export interface PaymentResponse {
  checkoutUrl: string
  paymentId: string
}

export interface BookedRange {
  startTime: string
  endTime: string
  reservationStatusName: string
}
