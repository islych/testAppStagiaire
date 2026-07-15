#!/usr/bin/env pwsh

# Script de diagnostic pour GestionDesStagiaires.Web

Write-Host "Diagnostic de GestionDesStagiaires.Web..." -ForegroundColor Cyan
Write-Host ""

# 1. Verifier .NET
Write-Host "1. Verification de .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
Write-Host "   Version .NET : $dotnetVersion" -ForegroundColor Green

# 2. Verifier les fichiers de configuration
Write-Host ""
Write-Host "2. Verification des fichiers de configuration..." -ForegroundColor Yellow

$appsettingsPath = "appsettings.Development.json"
if (Test-Path $appsettingsPath) {
    Write-Host "   OK - $appsettingsPath existe" -ForegroundColor Green
} else {
    Write-Host "   ERREUR - $appsettingsPath non trouve" -ForegroundColor Red
}

# 3. Verifier la structure des dossiers
Write-Host ""
Write-Host "3. Verification de la structure..." -ForegroundColor Yellow

$dirs = @("Pages", "Services", "Models", "wwwroot")
foreach ($dir in $dirs) {
    if (Test-Path $dir) {
        Write-Host "   OK - Dossier /$dir" -ForegroundColor Green
    }
}

# 4. Verifier les certificats HTTPS
Write-Host ""
Write-Host "4. Verification des certificats HTTPS..." -ForegroundColor Yellow
Write-Host "   Commande pour faire confiance au certificat :" -ForegroundColor Yellow
Write-Host "   dotnet dev-certs https --trust" -ForegroundColor Cyan

# 5. Tester la connectivite
Write-Host ""
Write-Host "5. Tests de connectivite..." -ForegroundColor Yellow

$endpoints = @{
    "Authentication" = "https://localhost:7001/api/v1/home/health"
    "Candidatures" = "https://localhost:7002/api/v1/home/health"
}

foreach ($name in $endpoints.Keys) {
    $uri = $endpoints[$name]
    try {
        $response = Invoke-WebRequest -Uri $uri -SkipCertificateCheck -TimeoutSec 3 -ErrorAction Stop
        Write-Host "   OK - $name accessible" -ForegroundColor Green
    } catch {
        Write-Host "   ERREUR - $name non accessible" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Diagnostic termine" -ForegroundColor Green

