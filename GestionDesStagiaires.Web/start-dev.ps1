# Script de demarrage en mode Development

Write-Host "Demarrage de GestionDesStagiaires.Web en mode Development..." -ForegroundColor Cyan

# Definir l'environnement Development
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Verifier les URLs
Write-Host ""
Write-Host "Configuration des APIs :" -ForegroundColor Yellow
Write-Host "  - Authentication.Service : https://localhost:7001" -ForegroundColor Cyan
Write-Host "  - Candidatures.Service   : https://localhost:7002" -ForegroundColor Cyan
Write-Host ""

# Lancer l'application
Write-Host "Lancement de l'application..." -ForegroundColor Green
dotnet run
