# Verifica que los servicios de Rentify estén escuchando en sus puertos
$services = @(
    @{ Port = 5000; Name = "ApiGateway" },
    @{ Port = 5093; Name = "AuthMS" },
    @{ Port = 5053; Name = "BranchOfficeMS" },
    @{ Port = 5054; Name = "VehicleMS" },
    @{ Port = 5055; Name = "ReservationMS" },
    @{ Port = 5099; Name = "PaymentMS" },
    @{ Port = 5173; Name = "Frontend (Vite)" }
)

Write-Host ""
Write-Host "=== Estado de Rentify ===" -ForegroundColor Cyan
$allOk = $true

foreach ($svc in $services) {
    $conn = Get-NetTCPConnection -LocalPort $svc.Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($conn) {
        Write-Host "[OK]   $($svc.Name) -> puerto $($svc.Port)" -ForegroundColor Green
    } else {
        Write-Host "[FALTA] $($svc.Name) -> puerto $($svc.Port)" -ForegroundColor Red
        $allOk = $false
    }
}

Write-Host ""
if ($allOk) {
    Write-Host "Todo listo. Abrí http://localhost:5173" -ForegroundColor Green
} else {
    Write-Host "Faltan servicios. Revisá las ventanas de PowerShell que abrió start-dev.ps1" -ForegroundColor Yellow
    Write-Host "NO cierres esas ventanas mientras uses la app." -ForegroundColor Yellow
    Write-Host "El frontend necesita ApiGateway (5000) para funcionar." -ForegroundColor Yellow
}
