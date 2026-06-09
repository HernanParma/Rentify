# Script para levantar todos los microservicios de Rentify en ventanas separadas
$root = $PSScriptRoot

$services = @(
    @{ Name = "VehicleMS";       Path = "$root\VehicleMS\VehicleMS";       Port = 5054 },
    @{ Name = "BranchOfficeMS";  Path = "$root\BranchOfficeMS\BranchOfficeMS"; Port = 5053 },
    @{ Name = "ReservationMS";   Path = "$root\ReservationMS\ReservationMS"; Port = 5055 },
    @{ Name = "AuthMS";          Path = "$root\AuthMS\AuthMS";          Port = 5093 },
    @{ Name = "PaymentMS";       Path = "$root\PaymentMS\PaymentMS";       Port = 5099 },
    @{ Name = "ApiGateway";      Path = "$root\ApiGateway";             Port = 5000 }
)

Write-Host "Iniciando Rentify..." -ForegroundColor Cyan
Write-Host "Se abrira una ventana por servicio. NO las cierres mientras uses la app." -ForegroundColor Yellow
Write-Host ""

foreach ($svc in $services) {
    Write-Host "Iniciando $($svc.Name) (puerto $($svc.Port))..."
    Start-Process powershell -ArgumentList @(
        "-NoExit",
        "-Command",
        "cd '$($svc.Path)'; Write-Host '=== $($svc.Name) ===' -ForegroundColor Cyan; dotnet run"
    )
    Start-Sleep -Seconds 5
}

Write-Host "Esperando compilacion inicial (30s)..."
Start-Sleep -Seconds 30

Write-Host "Iniciando Frontend..."
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd '$root\frontend'; Write-Host '=== Frontend ===' -ForegroundColor Cyan; npm run dev"
)

Start-Sleep -Seconds 8
& "$root\check-ports.ps1"

Write-Host ""
Write-Host "Usuario demo: demo@rentify.com / Demo123!" -ForegroundColor Cyan
Write-Host "Admin: admin@rentify.com / Demo123!" -ForegroundColor Cyan
Write-Host ""
Write-Host "Si falta algun servicio, mirá el error en su ventana de PowerShell." -ForegroundColor Yellow
Write-Host "No ejecutes 'dotnet run' de AuthMS aparte si ya lo levanto este script." -ForegroundColor Yellow
