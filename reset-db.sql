-- =========================================================
-- Script de nettoyage des données de test
-- Exécuter sur: GestionStages_Candidatures ET GestionStages_Documents
-- =========================================================

-- 1. Vider la table Candidatures
USE GestionStages_Candidatures;
GO
DELETE FROM Candidatures;
GO

-- 2. Vider la table Documents
USE GestionStages_Documents;
GO
DELETE FROM Documents;
GO

PRINT 'Nettoyage terminé.';
