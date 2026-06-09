import AvailabilityCalendar from './AvailabilityCalendar'
import './ReservationDatesModal.css'

interface Props {
  startDate: string
  endDate: string
  startTime: string
  endTime: string
  datesValid: boolean
  onRangeChange: (start: string, end: string) => void
  onStartTimeChange: (time: string) => void
  onEndTimeChange: (time: string) => void
  onConfirm: () => void
  onClose: () => void
}

export default function ReservationDatesModal({
  startDate,
  endDate,
  startTime,
  endTime,
  datesValid,
  onRangeChange,
  onStartTimeChange,
  onEndTimeChange,
  onConfirm,
  onClose,
}: Props) {
  return (
    <div className="reservation-dates-overlay" onClick={onClose}>
      <div className="reservation-dates-modal" onClick={(e) => e.stopPropagation()}>
        <button type="button" className="reservation-dates-close" onClick={onClose} aria-label="Cerrar">
          ×
        </button>

        <header className="reservation-dates-header">
          <h2>Elegí tus fechas</h2>
          <p>Seleccioná el período de alquiler para ver disponibilidad en el mapa.</p>
        </header>

        <AvailabilityCalendar
          bookedRanges={[]}
          startDate={startDate}
          endDate={endDate}
          onRangeChange={onRangeChange}
        />

        <div className="reservation-dates-times">
          <label>
            Hora retiro
            <input type="time" value={startTime} onChange={(e) => onStartTimeChange(e.target.value)} />
          </label>
          <label>
            Hora devolución
            <input type="time" value={endTime} onChange={(e) => onEndTimeChange(e.target.value)} />
          </label>
        </div>

        <div className="reservation-dates-actions">
          <button type="button" className="reservation-dates-cancel" onClick={onClose}>
            Ver mapa
          </button>
          <button
            type="button"
            className="reservation-dates-confirm"
            onClick={onConfirm}
            disabled={!datesValid}
          >
            Confirmar fechas
          </button>
        </div>
      </div>
    </div>
  )
}
