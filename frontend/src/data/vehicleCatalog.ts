export interface VehicleSpecs {
  category: string
  transmission: string
  fuel: string
  seats: number
  luggage: string
}

export interface VehicleCatalogEntry {
  imageUrl: string
  images: string[]
  description: string
  specs: VehicleSpecs
}

function slug(brand: string, model: string): string {
  return `${brand}-${model}`.toLowerCase().replace(/\s+/g, '-')
}

function imgs(slugKey: string, count = 3): string[] {
  return Array.from({ length: count }, (_, i) => `/vehicles/${slugKey}/${i + 1}.jpg`)
}

const catalog: Record<string, VehicleCatalogEntry> = {
  'toyota-corolla': {
    imageUrl: '/vehicles/toyota-corolla/1.jpg',
    images: imgs('toyota-corolla'),
    description:
      'Toyota Corolla sedán/hatch — referente en confiabilidad y eficiencia. Motor híbrido o naftero, amplio baúl y equipamiento de seguridad Toyota Safety Sense. Ideal para ciudad, ruta y viajes de negocios.',
    specs: { category: 'Sedán', transmission: 'Automática CVT', fuel: 'Nafta / Híbrido', seats: 5, luggage: '470 L' },
  },
  'ford-ranger': {
    imageUrl: '/vehicles/ford-ranger/1.jpg',
    images: imgs('ford-ranger'),
    description:
      'Ford Ranger pick-up doble cabina — potencia Bi-Turbo, tracción 4x4 y suspensión reforzada. Capacidad de carga superior y despeje para caminos de tierra, camping o trabajo.',
    specs: { category: 'Pick-up', transmission: 'Automática 10 vel.', fuel: 'Diésel', seats: 5, luggage: 'Caja 1.500 L' },
  },
  'volkswagen-gol': {
    imageUrl: '/vehicles/volkswagen-gol/1.jpg',
    images: imgs('volkswagen-gol'),
    description:
      'Volkswagen Gol hatch — compacto best-seller en Argentina. Ágil en tránsito urbano, bajo consumo y mantenimiento accesible. Perfecto para uso diario y estacionamiento fácil.',
    specs: { category: 'Hatchback', transmission: 'Manual 5 vel.', fuel: 'Nafta', seats: 5, luggage: '285 L' },
  },
  'honda-civic': {
    imageUrl: '/vehicles/honda-civic/1.jpg',
    images: imgs('honda-civic'),
    description:
      'Honda Civic generación actual — diseño deportivo, interior premium y manejo preciso. Tecnología Honda Sensing, pantalla multimedia y excelente comportamiento en ruta.',
    specs: { category: 'Sedán', transmission: 'Automática CVT', fuel: 'Nafta / Híbrido', seats: 5, luggage: '410 L' },
  },
  'chevrolet-onix': {
    imageUrl: '/vehicles/chevrolet-onix/1.jpg',
    images: imgs('chevrolet-onix'),
    description:
      'Chevrolet Onix hatch — líder de ventas en el segmento B. Conectividad MyLink, buen espacio interior y eficiencia de combustible. Muy popular para familias jóvenes.',
    specs: { category: 'Hatchback', transmission: 'Manual / Automática', fuel: 'Nafta', seats: 5, luggage: '303 L' },
  },
  'fiat-cronos': {
    imageUrl: '/vehicles/fiat-cronos/1.jpg',
    images: imgs('fiat-cronos'),
    description:
      'Fiat Cronos sedán — derivado del Argo con mayor baúl y confort en ruta. Suspensión elevada tipo crossover, ideal para familias que combinan ciudad y viajes.',
    specs: { category: 'Sedán', transmission: 'Manual / Automática', fuel: 'Nafta', seats: 5, luggage: '525 L' },
  },
  'renault-kwid': {
    imageUrl: '/vehicles/renault-kwid/1.jpg',
    images: imgs('renault-kwid'),
    description:
      'Renault Kwid SUV urbano — el más compacto y económico de la flota. Alto despeje, diseño aventurero y consumo reducido. Ideal para primer auto o traslados urbanos.',
    specs: { category: 'SUV compacto', transmission: 'Manual 5 vel.', fuel: 'Nafta', seats: 5, luggage: '290 L' },
  },
  'peugeot-208': {
    imageUrl: '/vehicles/peugeot-208/1.jpg',
    images: imgs('peugeot-208'),
    description:
      'Peugeot 208 hatch — estilo francés, i-Cockpit y conducción dinámica. Diseño vanguardista con opciones nafta o eléctricas según mercado. Perfecto para quienes buscan diseño.',
    specs: { category: 'Hatchback', transmission: 'Manual / Automática', fuel: 'Nafta / Eléctrico', seats: 5, luggage: '311 L' },
  },
  'toyota-hilux': {
    imageUrl: '/vehicles/toyota-hilux/1.jpg',
    images: imgs('toyota-hilux'),
    description:
      'Toyota Hilux pick-up — legendaria resistencia y tracción 4x4. Motor diésel potente, chasis reforzado y capacidad off-road. Referente para trabajo, campo y aventura.',
    specs: { category: 'Pick-up', transmission: 'Manual / Automática', fuel: 'Diésel', seats: 5, luggage: 'Caja 1.200 L' },
  },
  'nissan-versa': {
    imageUrl: '/vehicles/nissan-versa/1.jpg',
    images: imgs('nissan-versa'),
    description:
      'Nissan Versa sedán — espacio interior generoso y baúl amplio. Asientos cómodos, buena relación precio-equipamiento. Recomendado para familias y viajes largos.',
    specs: { category: 'Sedán', transmission: 'Manual / CVT', fuel: 'Nafta', seats: 5, luggage: '480 L' },
  },
  'jeep-renegade': {
    imageUrl: '/vehicles/jeep-renegade/1.jpg',
    images: imgs('jeep-renegade'),
    description:
      'Jeep Renegade SUV — carácter aventurero con tracción 4x4 disponible. Diseño icónico Jeep, buen despeje y versatilidad urbana/rural. Para quienes quieren estilo y capacidad.',
    specs: { category: 'SUV compacto', transmission: 'Automática 6 vel.', fuel: 'Nafta / Diésel', seats: 5, luggage: '351 L' },
  },
  'volkswagen-amarok': {
    imageUrl: '/vehicles/volkswagen-amarok/1.jpg',
    images: imgs('volkswagen-amarok'),
    description:
      'Volkswagen Amarok pick-up premium — cabina doble V6, acabados superiores y gran capacidad de remolque. Combina lujo, potencia y utilidad para viajes exigentes.',
    specs: { category: 'Pick-up', transmission: 'Automática 8 vel.', fuel: 'Diésel V6', seats: 5, luggage: 'Caja 1.550 L' },
  },
  'fiat-argo': {
    imageUrl: '/vehicles/fiat-argo/1.jpg',
    images: imgs('fiat-argo'),
    description:
      'Fiat Argo hatch — moderno, eficiente y muy vendido en Argentina. Buena conectividad Uconnect, maniobrabilidad urbana y bajo costo operativo.',
    specs: { category: 'Hatchback', transmission: 'Manual / Automática', fuel: 'Nafta', seats: 5, luggage: '300 L' },
  },
  'ford-ecosport': {
    imageUrl: '/vehicles/ford-ecosport/1.jpg',
    images: imgs('ford-ecosport'),
    description:
      'Ford EcoSport SUV compacto — altura elevada, buena visibilidad y manejo ágil. Versión ST-Line con look deportivo. Equilibrio entre SUV y consumo urbano.',
    specs: { category: 'SUV compacto', transmission: 'Manual / Automática', fuel: 'Nafta / Diésel', seats: 5, luggage: '355 L' },
  },
  'chevrolet-cruze': {
    imageUrl: '/vehicles/chevrolet-cruze/1.jpg',
    images: imgs('chevrolet-cruze'),
    description:
      'Chevrolet Cruze sedán — segmento C con terminaciones superiores, motor turbo y manejo refinado. Confort en ruta y equipamiento OnStar. Para ejecutivos y familias exigentes.',
    specs: { category: 'Sedán', transmission: 'Automática 6 vel.', fuel: 'Nafta Turbo', seats: 5, luggage: '445 L' },
  },
}

const defaultEntry: VehicleCatalogEntry = {
  imageUrl: '/vehicles/toyota-corolla/1.jpg',
  images: imgs('toyota-corolla'),
  description: 'Vehículo disponible para alquiler con seguro incluido y mantenimiento al día.',
  specs: { category: 'Automóvil', transmission: 'Automática', fuel: 'Nafta', seats: 5, luggage: '300 L' },
}

export function getVehicleCatalogEntry(brand: string, model: string): VehicleCatalogEntry {
  const key = slug(brand, model)
  return catalog[key] ?? {
    ...defaultEntry,
    imageUrl: defaultEntry.imageUrl,
    images: defaultEntry.images,
    description: `${brand} ${model}: vehículo disponible para alquiler con seguro incluido y mantenimiento al día.`,
  }
}

export function getVehicleSlug(brand: string, model: string): string {
  return slug(brand, model)
}
