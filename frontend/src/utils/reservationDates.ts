export interface BookedRange {
  startTime: string
  endTime: string
  reservationStatusName: string
}

export function rangesOverlap(start: Date, end: Date, range: BookedRange): boolean {
  const rangeStart = new Date(range.startTime)
  const rangeEnd = new Date(range.endTime)
  return start < rangeEnd && end > rangeStart
}

export function findBookingConflict(start: Date, end: Date, ranges: BookedRange[]): BookedRange | null {
  return ranges.find((r) => rangesOverlap(start, end, r)) ?? null
}

export function formatBookedRange(range: BookedRange): string {
  const start = new Date(range.startTime)
  const end = new Date(range.endTime)
  const opts: Intl.DateTimeFormatOptions = { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' }
  return `${start.toLocaleString('es-AR', opts)} → ${end.toLocaleString('es-AR', opts)}`
}
