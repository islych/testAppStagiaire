using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Documents.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Documents",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StagiaireId = table.Column<int>(type: "int", nullable: false),
                    CandidatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NomFichier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NomFichierStockage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CheminFichier = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TailleFichierOctets = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "EnAttente"),
                    VerificateurId = table.Column<int>(type: "int", nullable: true),
                    CommentaireVerificateur = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DateDepot = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DateValidation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateMiseAJour = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstVersionCourante = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DocumentPrecedentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Document_CandidatureId",
                schema: "dbo",
                table: "Documents",
                column: "CandidatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_EstVersionCourante",
                schema: "dbo",
                table: "Documents",
                column: "EstVersionCourante");

            migrationBuilder.CreateIndex(
                name: "IX_Document_StagiaireId",
                schema: "dbo",
                table: "Documents",
                column: "StagiaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_StagiaireId_Type_Version",
                schema: "dbo",
                table: "Documents",
                columns: new[] { "StagiaireId", "Type", "EstVersionCourante" });

            migrationBuilder.CreateIndex(
                name: "IX_Document_Statut",
                schema: "dbo",
                table: "Documents",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Document_Type",
                schema: "dbo",
                table: "Documents",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents",
                schema: "dbo");
        }
    }
}
