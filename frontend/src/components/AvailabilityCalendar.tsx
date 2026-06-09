import { useMemo, useState } from 'react'
import type { BookedRange } from '../types'
import { rangesOverlap } from '../utils/reservationDates'
import './AvailabilityCalendar.css'

const WEEKDAYS = ['Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sa', 'Do']
const MONTHS = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre',
]

function pad(n: number) {
  return String(n).padStart(2, '0')
}

export function toDateKey(d: Date) {
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

function parseDateKey(key: string) {
  const [y, m, d] = key.split('-').map(Number)
  return new Date(y, m - 1, d)
}

function isDayBooked(day: Date, ranges: BookedRange[]) {
  const dayStart = new Date(day.getFullYear(), day.getMonth(), day.getDate(), 0, 0, 0, 0)
  const dayEnd = new Date(day.getFullYear(), day.getMonth(), day.getDate(), 23, 59, 59, 999)
  return ranges.some((r) => rangesOverlap(dayStart, dayEnd, r))
}

function isDayPast(day: Date) {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const check = new Date(day.getFullYear(), day.getMonth(), day.getDate())
  return check < today
}

interface Props {
  bookedRanges: BookedRange[]
  startDate: string
  endDate: string
  onRangeChange: (start: string, end: string) => void
}

export default function AvailabilityCalendar({
  bookedRanges,
  startDate,
  endDate,
  onRangeChange,
}: Props) {
  const today = new Date()
  const [viewMonth, setViewMonth] = useState(today.getMonth())
  const [viewYear, setViewYear] = useState(today.getFullYear())
  const [pickingEnd, setPickingEnd] = useState(false)

  const days = useMemo(() => {
    const first = new Date(viewYear, viewMonth, 1)
    const startPad = (first.getDay() + 6) % 7
    const gridStart = new Date(viewYear, viewMonth, 1 - startPad)
    return Array.from({ length: 42 }, (_, i) => {
      const d = new Date(gridStart)
      d.setDate(gridStart.getDate() + i)
      return d
    })
  }, [viewMonth, viewYear])

  const shiftMonth = (delta: number) => {
    const d = new Date(viewYear, viewMonth + delta, 1)
    setViewMonth(d.getMonth())
    setViewYear(d.getFullYear())
  }

  const handleDayClick = (day: Date) => {
    if (isDayPast(day) || isDayBooked(day, bookedRanges)) return
    const key = toDateKey(day)

    if (!pickingEnd || !startDate || key < startDate) {
      onRangeChange(key, key)
      setPickingEnd(true)
      return
    }

    const from = parseDateKey(startDate)
    const to = day
    const cursor = new Date(Math.min(from.getTime(), to.getTime()))
    const limit = new Date(Math.max(from.getTime(), to.getTime()))
    while (cursor <= limit) {
      if (isDayBooked(cursor, bookedRanges)) return
      cursor.setDate(cursor.getDate() + 1)
    }

    onRangeChange(startDate, key)
    setPickingEnd(false)
  }

  const getDayClass = (day: Date) => {
    const key = toDateKey(day)
    const inMonth = day.getMonth() === viewMonth
    const past = isDayPast(day)
    const booked = isDayBooked(day, bookedRanges)
    const selected =
      startDate &&
      endDate &&
      key >= startDate &&
      key <= endDate

    const classes = ['cal-day']
    if (!inMonth) classes.push('cal-day--muted')
    if (past) classes.push('cal-day--past')
    else if (booked) classes.push('cal-day--booked')
    else classes.push('cal-day--free')
    if (selected) classes.push('cal-day--selected')
    if (key === startDate || key === endDate) classes.push('cal-day--edge')
    return classes.join(' ')
  }

  return (
    <div className="availability-calendar">
      <div className="cal-header">
        <button type="button" onClick={() => shiftMonth(-1)} aria-label="Mes anterior">‹</button>
        <strong>{MONTHS[viewMonth]} {viewYear}</strong>
        <button type="button" onClick={() => shiftMonth(1)} aria-label="Mes siguiente">›</button>
      </div>

      <div className="cal-weekdays">
        {WEEKDAYS.map((w) => (
          <span key={w}>{w}</span>
        ))}
      </div>

      <div className="cal-grid">
        {days.map((day) => {
          const key = toDateKey(day)
          const disabled = isDayPast(day) || isDayBooked(day, bookedRanges)
          return (
            <button
              key={key}
              type="button"
              className={getDayClass(day)}
              disabled={disabled}
              onClick={() => handleDayClick(day)}
              title={
                isDayBooked(day, bookedRanges)
                  ? 'Día reservado'
                  : isDayPast(day)
                    ? 'Fecha pasada'
                    : 'Disponible'
              }
            >
              {day.getDate()}
            </button>
          )
        })}
      </div>

      <div className="cal-legend">
        <span><i className="dot dot--free" /> Disponible</span>
        <span><i className="dot dot--booked" /> Reservado</span>
        <span><i className="dot dot--selected" /> Tu selección</span>
      </div>

      <p className="cal-hint">
        {pickingEnd && startDate
          ? 'Elegí la fecha de fin en un día verde.'
          : 'Elegí la fecha de inicio en un día verde.'}
      </p>

      {startDate && endDate && (
        <p className="cal-selection">
          Seleccionado: {parseDateKey(startDate).toLocaleDateString('es-AR')}
          {startDate !== endDate && (
            <> → {parseDateKey(endDate).toLocaleDateString('es-AR')}</>
          )}
        </p>
      )}
    </div>
  )
}
