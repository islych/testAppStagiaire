@echo off
REM Script de demarrage en mode Development pour Windows CMD

cls
echo.
echo ========================================
echo Demarrage de GestionDesStagiaires.Web
echo Mode: DEVELOPMENT
echo ========================================
echo.

REM Definir l'environnement Development
set ASPNETCORE_ENVIRONMENT=Development

echo Configuration des APIs :
echo   - Authentication.Service : https://localhost:7001
echo   - Candidatures.Service   : https://localhost:7002
echo.

echo Verification des services actifs (optionnel)
echo   Assurez-vous que les 2 microservices sont actifs
echo.

REM Lancer l'application
echo Lancement de l'application en mode Development...
echo.

dotnet run

pause
