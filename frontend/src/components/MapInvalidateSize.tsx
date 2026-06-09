import { useEffect } from 'react'
import { useMap } from 'react-leaflet'

interface Props {
  trigger: string
}

export default function MapInvalidateSize({ trigger }: Props) {
  const map = useMap()

  useEffect(() => {
    const invalidate = () => {
      map.invalidateSize({ animate: false })
    }

    invalidate()
    const t1 = window.setTimeout(invalidate, 0)
    const t2 = window.setTimeout(invalidate, 150)
    const t3 = window.setTimeout(invalidate, 400)

    window.addEventListener('resize', invalidate)

    return () => {
      window.clearTimeout(t1)
      window.clearTimeout(t2)
      window.clearTimeout(t3)
      window.removeEventListener('resize', invalidate)
    }
  }, [map, trigger])

  return null
}
