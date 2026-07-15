#!/usr/bin/env pwsh

# Script de nettoyage pour GestionDesStagiaires.Web
# Résout les problèmes de session et de clés de protection

Write-Host "🧹 Nettoyage de GestionDesStagiaires.Web..." -ForegroundColor Cyan

# Arrêter l'application si elle s'exécute
Write-Host "1. Arrêt de l'application (si en cours)..." -ForegroundColor Yellow

# Nettoyer bin et obj
Write-Host "2. Suppression des dossiers bin et obj..." -ForegroundColor Yellow
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "   ✓ Fait" -ForegroundColor Green

# Nettoyer les clés de protection de données
Write-Host "3. Suppression des clés de protection de données..." -ForegroundColor Yellow
Remove-Item -Path "keys" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "   ✓ Fait" -ForegroundColor Green

# Nettoyer les données temporaires
Write-Host "4. Suppression des fichiers temporaires..." -ForegroundColor Yellow
Remove-Item -Path ".vs" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "   ✓ Fait" -ForegroundColor Green

# Restaurer les dépendances
Write-Host "5. Restauration des dépendances NuGet..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ❌ Erreur lors de la restauration" -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Fait" -ForegroundColor Green

# Compiler
Write-Host "6. Compilation du projet..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ❌ Erreur lors de la compilation" -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Fait" -ForegroundColor Green

Write-Host ""
Write-Host "✅ Nettoyage terminé avec succès !" -ForegroundColor Green
Write-Host ""
Write-Host "Prochaines étapes :" -ForegroundColor Cyan
Write-Host "  1. Nettoyer les cookies du navigateur (F12 → Application → Cookies → Supprimer tous)"
Write-Host "  2. Lancer l'application : dotnet run"
Write-Host "  3. Essayer de se connecter : https://localhost:5001/account/login"
Write-Host ""
