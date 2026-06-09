const STORAGE_KEY = 'rentify_hidden_reservations'

function readStore(): Record<string, string[]> {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return {}
    const parsed = JSON.parse(raw) as Record<string, string[]>
    return typeof parsed === 'object' && parsed !== null ? parsed : {}
  } catch {
    return {}
  }
}

function writeStore(store: Record<string, string[]>) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(store))
}

export function getHiddenReservationIds(userId: number): Set<string> {
  const store = readStore()
  return new Set(store[String(userId)] ?? [])
}

export function hideReservationFromView(userId: number, reservationId: string) {
  const store = readStore()
  const key = String(userId)
  const current = new Set(store[key] ?? [])
  current.add(reservationId)
  store[key] = [...current]
  writeStore(store)
}
