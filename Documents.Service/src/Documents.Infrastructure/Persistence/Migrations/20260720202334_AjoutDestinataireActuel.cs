using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Documents.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjoutDestinataireActuel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ajouter la colonne si elle n'existe pas encore
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME   = 'Documents'
                      AND COLUMN_NAME  = 'DestinataireActuel'
                )
                BEGIN
                    ALTER TABLE [dbo].[Documents]
                        ADD [DestinataireActuel] NVARCHAR(50) NOT NULL
                        CONSTRAINT DF_Documents_DestinataireActuel DEFAULT 'Encadrant';
                END
                ELSE
                BEGIN
                    -- La colonne existe : mettre à jour les NULL et ajouter la contrainte DEFAULT
                    UPDATE [dbo].[Documents]
                        SET [DestinataireActuel] = 'Encadrant'
                        WHERE [DestinataireActuel] IS NULL;

                    IF NOT EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA   = 'dbo'
                          AND TABLE_NAME     = 'Documents'
                          AND COLUMN_NAME    = 'DestinataireActuel'
                          AND IS_NULLABLE    = 'NO'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[Documents]
                            ALTER COLUMN [DestinataireActuel] NVARCHAR(50) NOT NULL;
                    END

                    IF NOT EXISTS (
                        SELECT 1 FROM sys.default_constraints
                        WHERE name = 'DF_Documents_DestinataireActuel'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[Documents]
                            ADD CONSTRAINT DF_Documents_DestinataireActuel
                            DEFAULT 'Encadrant' FOR [DestinataireActuel];
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.default_constraints
                    WHERE name = 'DF_Documents_DestinataireActuel'
                )
                BEGIN
                    ALTER TABLE [dbo].[Documents]
                        DROP CONSTRAINT DF_Documents_DestinataireActuel;
                END

                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'dbo'
                      AND TABLE_NAME   = 'Documents'
                      AND COLUMN_NAME  = 'DestinataireActuel'
                )
                BEGIN
                    ALTER TABLE [dbo].[Documents]
                        DROP COLUMN [DestinataireActuel];
                END
            ");
        }
    }
}
