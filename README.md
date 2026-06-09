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
4. **Certificado HTTPS de desarrollo** (una sola vez):
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

## Mercado Pago (pagos reales)

Para que el checkout funcione necesitás un **Access Token de prueba** de Mercado Pago:

1. Creá cuenta en [Mercado Pago Developers](https://www.mercadopago.com.ar/developers)
2. Obtené tu **Test Access Token**
3. Configuralo en `PaymentMS/PaymentMS/appsettings.json`:
   ```json
   "MercadoPago": {
     "AccessToken": "TU_TOKEN_DE_PRUEBA",
     "BackUrlBase": "http://localhost:5173"
   }
   ```
4. Usá [tarjetas de prueba de MP](https://www.mercadopago.com.ar/developers/es/docs/checkout-pro/additional-content/test-cards) para simular pagos

Sin token válido, la reserva se crea pero el botón de pago fallará.

## Configuración AuthMS (User Secrets)

AuthMS requiere JWT key y connection string en User Secrets (modo DEBUG):

```powershell
cd AuthMS\AuthMS
dotnet user-secrets set "JwtSettings:key" "RentifySecretKey2024Minimo32Caracteres!"
dotnet user-secrets set "ConnectionString" "Server=localhost;Database=Autenticacion;Trusted_Connection=True;TrustServerCertificate=True;"
```

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
| AuthMS no arranca | Configurar User Secrets (ver arriba) |
| Mapa vacío | Verificar que VehicleMS y BranchOfficeMS estén corriendo |
| Error al reservar | ReservationMS necesita VehicleMS activo |
| Error al pagar | Configurar token de Mercado Pago |
| CORS error | Verificar que ApiGateway esté en puerto 7000 |
| Frontend no conecta | Revisar `frontend/.env` → `VITE_API_URL=https://localhost:7000` |
