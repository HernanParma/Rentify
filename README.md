# Rentify - Alquiler de Autos (Microservicios)

Aplicación de alquiler de vehículos dividida en microservicios .NET 8 con frontend React.

## Arquitectura

| Microservicio | Puerto HTTPS | Responsabilidad |
|---------------|--------------|-----------------|
| **ApiGateway** | 7000 | Punto de entrada único (YARP) |
| **AuthMS** | 7052 | Autenticación, usuarios, JWT |
| **BranchOfficeMS** | 7053 | Sucursales y mapa |
| **VehicleMS** | 7054 | Flota de vehículos |
| **ReservationMS** | 7055 | Reservas |
| **PaymentMS** | 7059 | Pagos (Mercado Pago) |
| **Frontend** | 5173 | Login, mapa, reservas, checkout |

## Requisitos previos

1. **.NET 8 SDK** — [Descargar](https://dotnet.microsoft.com/download)
2. **SQL Server** — LocalDB o SQL Server Express (debe estar corriendo)
3. **Node.js 18+** — Para el frontend
4. **Secretos locales** (obligatorio la primera vez):
   ```powershell
   cd c:\Users\herna\Desktop\PROYECTOS\Rentify
   .\setup-secrets.ps1
   ```
   Genera `appsettings.Local.json` en cada microservicio (no se sube a Git) y configura User Secrets de AuthMS.
5. **Certificado HTTPS de desarrollo** (una sola vez):
   ```powershell
   dotnet dev-certs https --trust
   ```

## Cómo ver la app en local (paso a paso)

### Opción A: Script automático

```powershell
cd c:\Users\herna\Desktop\PROYECTOS\Rentify
.\start-dev.ps1
```

Esto abre 7 terminales (6 microservicios + frontend). Esperá ~30 segundos a que todos arranquen.

### Opción B: Manual (recomendado la primera vez)

Abrí **7 terminales** y ejecutá en este orden (esperá que cada uno muestre "Now listening on..."):

```powershell
# Terminal 1 - Vehículos (otros MS dependen de este)
cd c:\Users\herna\Desktop\PROYECTOS\Rentify\VehicleMS\VehicleMS
dotnet run

# Terminal 2 - Sucursales
cd c:\Users\herna\Desktop\PROYECTOS\Rentify\BranchOfficeMS\BranchOfficeMS
dotnet run

# Terminal 3 - Reservas
cd c:\Users\herna\Desktop\PROYECTOS\Rentify\ReservationMS\ReservationMS
dotnet run

# Terminal 4 - Autenticación
cd c:\Users\herna\Desktop\PROYECTOS\Rentify\AuthMS\AuthMS
dotnet run

# Terminal 5 - Pagos
cd c:\Users\herna\Desktop\PROYECTOS\Rentify\PaymentMS\PaymentMS
dotnet run

# Terminal 6 - Gateway (entrada única)
cd c:\Users\herna\Desktop\PROYECTOS\Rentify\ApiGateway
dotnet run

# Terminal 7 - Frontend
cd c:\Users\herna\Desktop\PROYECTOS\Rentify\frontend
npm install
npm run dev
```

### Abrir la app

1. Navegador → **http://localhost:5173**
2. Login con usuario demo:
   - **Email:** `demo@rentify.com`
   - **Contraseña:** `Demo123!`
3. Explorá el mapa, elegí una sede, click en **Reservar** en un auto
4. Elegí fechas y sucursal de devolución → **Continuar al pago**
5. En checkout → **Pagar con Mercado Pago**

### Swagger (probar APIs directamente)

| Servicio | URL |
|----------|-----|
| Gateway health | https://localhost:7000/health |
| AuthMS | https://localhost:7052/swagger |
| BranchOfficeMS | https://localhost:7053/swagger |
| VehicleMS | https://localhost:7054/swagger |
| ReservationMS | https://localhost:7055/swagger |
| PaymentMS | https://localhost:7059/swagger |

## Flujo completo implementado

```
Login → Mapa de sedes → Elegir auto → Reservar (fechas)
  → Checkout → Mercado Pago → Pago exitoso → Mis reservas
```

## Usuarios demo

| Email | Contraseña | Rol |
|-------|------------|-----|
| demo@rentify.com | Demo123! | Customer |
| admin@rentify.com | Demo123! | Admin |

Se crean automáticamente al iniciar AuthMS en modo Development.

## Secretos y configuración sensible

**No commitear** claves JWT, tokens de Mercado Pago ni contraseñas de base de datos.

| Archivo | En Git | Uso |
|---------|--------|-----|
| `appsettings.json` | Sí | Valores por defecto sin secretos |
| `appsettings.Local.json.example` | Sí | Plantilla |
| `appsettings.Local.json` | **No** | Secretos locales (`.\setup-secrets.ps1`) |
| `frontend/.env` | **No** | Variables del frontend |
| `frontend/.env.example` | Sí | Plantilla |

### Mercado Pago (pagos reales)

Por defecto `UseMockPayments: true`. Para pagos de prueba reales, editá `PaymentMS/PaymentMS/appsettings.Local.json` con tu [Test Access Token](https://www.mercadopago.com.ar/developers) y `"UseMockPayments": false`.

### Rotación de claves

Si una clave estuvo en GitHub, generá una nueva: borrá los `appsettings.Local.json` y ejecutá `.\setup-secrets.ps1` de nuevo. El historial de Git puede conservar secretos antiguos; en repos públicos conviene rotar credenciales.

## Bases de datos

Cada microservicio crea su BD al iniciar:

| BD | Microservicio |
|----|---------------|
| Autenticacion | AuthMS |
| BranchOfficeDb | BranchOfficeMS |
| VehicleDb | VehicleMS |
| ReservationDb | ReservationMS |
| PaymentDb | PaymentMS |

## Estructura del proyecto

```
Rentify/
├── AuthMS/
├── BranchOfficeMS/
├── VehicleMS/
├── ReservationMS/
├── PaymentMS/
├── ApiGateway/
├── frontend/
├── start-dev.ps1
└── README.md
```

## Lo que falta implementar

### Prioridad alta
- [ ] **Marcar vehículo como "Rented"** al confirmar pago (VehicleMS no actualiza estado)
- [ ] **Validación JWT** en ReservationMS, VehicleMS y BranchOfficeMS (hoy son públicos)
- [ ] **Token real de Mercado Pago** en configuración del usuario
- [ ] **Docker Compose** para levantar SQL Server + todos los MS con un comando

### Prioridad media
- [ ] **Valoraciones** (VehicleReviews del DER)
- [ ] **Notificaciones de alquiler** (confirmación, recordatorio devolución) — AuthMS tiene infra de emails pero con tipos médicos
- [ ] **Panel Admin** — gestión de flota, sucursales, reservas
- [ ] **Pickup/Return** — registrar hora real de retiro y devolución
- [ ] **Cancelación de reservas**

### Prioridad baja
- [ ] **Refresh token** automático en frontend cuando expira el JWT
- [ ] **Tests** unitarios e integración
- [ ] **CI/CD** con GitHub Actions
- [ ] **Monorepo Git** unificado (hoy cada MS tiene su propio repo)

## Solución de problemas

| Problema | Solución |
|----------|----------|
| Error SSL / certificado | `dotnet dev-certs https --trust` |
| AuthMS no arranca | Ejecutar `.\setup-secrets.ps1` |
| JwtSettings:key no configurada | Ejecutar `.\setup-secrets.ps1` |
| Mapa vacío | Verificar que VehicleMS y BranchOfficeMS estén corriendo |
| Error al reservar | ReservationMS necesita VehicleMS activo |
| Error al pagar | Configurar token de Mercado Pago |
| CORS error | Verificar que ApiGateway esté en puerto 7000 |
| Frontend no conecta | Revisar `frontend/.env` → `VITE_API_URL=https://localhost:7000` |
