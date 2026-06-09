# Genera appsettings.Local.json (no se sube a Git) con clave JWT compartida para todos los microservicios.
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

function New-RentifyJwtKey {
    $bytes = New-Object byte[] 48
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    return [Convert]::ToBase64String($bytes)
}

function Ensure-LocalSecrets {
    param(
        [string]$ServicePath,
        [string]$ExampleFile,
        [hashtable]$Overrides = @{}
    )

    $localPath = Join-Path $ServicePath "appsettings.Local.json"
    $examplePath = Join-Path $ServicePath $ExampleFile

    if (-not (Test-Path $examplePath)) {
        throw "No se encontró $examplePath"
    }

    if (Test-Path $localPath) {
        Write-Host "  OK  $localPath (ya existe, no se modifica)" -ForegroundColor DarkGray
        return
    }

    $json = Get-Content $examplePath -Raw | ConvertFrom-Json

    foreach ($key in $Overrides.Keys) {
        if ($key -eq "JwtSettings:key") {
            if (-not $json.JwtSettings) { $json | Add-Member -NotePropertyName JwtSettings -NotePropertyValue (@{}) }
            $json.JwtSettings.key = $Overrides[$key]
        }
        elseif ($key -eq "ConnectionString") {
            $json | Add-Member -NotePropertyName ConnectionString -NotePropertyValue $Overrides[$key] -Force
        }
    }

    $json | ConvertTo-Json -Depth 5 | Set-Content $localPath -Encoding UTF8
    Write-Host "  +   $localPath" -ForegroundColor Green
}

Write-Host "Configurando secretos locales de Rentify..." -ForegroundColor Cyan

$jwtKey = New-RentifyJwtKey
$jwtOverride = @{ "JwtSettings:key" = $jwtKey }
$authOverride = @{
    "JwtSettings:key" = $jwtKey
    "ConnectionString" = "Server=localhost;Database=Autenticacion;Trusted_Connection=True;TrustServerCertificate=True;"
}

Ensure-LocalSecrets -ServicePath "$root\AuthMS\AuthMS" -ExampleFile "appsettings.Local.json.example" -Overrides $authOverride
Ensure-LocalSecrets -ServicePath "$root\VehicleMS\VehicleMS" -ExampleFile "appsettings.Local.json.example" -Overrides $jwtOverride
Ensure-LocalSecrets -ServicePath "$root\BranchOfficeMS\BranchOfficeMS" -ExampleFile "appsettings.Local.json.example" -Overrides $jwtOverride
Ensure-LocalSecrets -ServicePath "$root\ReservationMS\ReservationMS" -ExampleFile "appsettings.Local.json.example" -Overrides $jwtOverride
Ensure-LocalSecrets -ServicePath "$root\PaymentMS\PaymentMS" -ExampleFile "appsettings.Local.json.example"

$frontendEnv = Join-Path $root "frontend\.env"
$frontendExample = Join-Path $root "frontend\.env.example"
if (-not (Test-Path $frontendEnv) -and (Test-Path $frontendExample)) {
    Copy-Item $frontendExample $frontendEnv
    Write-Host "  +   frontend\.env" -ForegroundColor Green
}

# User Secrets de AuthMS (opcional, refuerza la misma clave en DEBUG)
Push-Location "$root\AuthMS\AuthMS"
dotnet user-secrets set "JwtSettings:key" $jwtKey | Out-Null
dotnet user-secrets set "ConnectionString" $authOverride.ConnectionString | Out-Null
Pop-Location
Write-Host "  OK  AuthMS user-secrets actualizados" -ForegroundColor Green

Write-Host ""
Write-Host "Listo. Los archivos appsettings.Local.json NO se suben a Git." -ForegroundColor Cyan
Write-Host "Si ya habías publicado secretos en GitHub, rotá la clave JWT (ya se generó una nueva local)." -ForegroundColor Yellow
