# Setup après git pull

Les fichiers `appsettings.Development.json` ne sont pas versionnés (gitignore).
Chaque développeur doit créer les siens avec son propre nom de serveur SQL.

## Étapes

### 1. Authentication.Service
Copier le template et adapter le nom du serveur :
```
Authentication.Service/src/Authentication.API/appsettings.Development.template.json
→ Authentication.Service/src/Authentication.API/appsettings.Development.json
```
Remplacer `TON_SERVEUR` par ton nom de machine (ex: `DESKTOP-VEFHU11` ou `localhost`).

### 2. Candidatures.Service
```
Candidatures.Service/src/Candidatures.API/appsettings.Development.template.json
→ Candidatures.Service/src/Candidatures.API/appsettings.Development.json
```
Remplacer `TON_SERVEUR` par ton nom de machine.

### 3. Trouver ton nom de serveur SQL
Dans SQL Server Management Studio, le nom du serveur est affiché à la connexion.
Ou dans PowerShell : `$env:COMPUTERNAME`

## Ordre de démarrage
1. Authentication.Service (port 5174)
2. Candidatures.Service (port 5296)
3. GestionDesStagiaires.Web (port 5000)
