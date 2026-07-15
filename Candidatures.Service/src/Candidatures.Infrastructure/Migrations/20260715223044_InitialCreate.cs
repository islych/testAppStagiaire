using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Candidatures.Infrastructure.Migrations
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
                name: "Candidatures",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StagiaireId = table.Column<int>(type: "int", nullable: false),
                    DepartementId = table.Column<int>(type: "int", nullable: false),
                    SpecialiteId = table.Column<int>(type: "int", nullable: false),
                    CvFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CvPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DureeStageMois = table.Column<int>(type: "int", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NiveauEtude = table.Column<int>(type: "int", nullable: false),
                    Ecole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LettreMotivation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "EnAttente"),
                    EncadrantId = table.Column<int>(type: "int", nullable: true),
                    Commentaire = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DateMiseAJour = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDecision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransmisADirection = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DateTransmissionDirection = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatures", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidature_EncadrantId",
                schema: "dbo",
                table: "Candidatures",
                column: "EncadrantId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidature_StagiaireId",
                schema: "dbo",
                table: "Candidatures",
                column: "StagiaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidature_StagiaireId_Statut",
                schema: "dbo",
                table: "Candidatures",
                columns: new[] { "StagiaireId", "Statut" });

            migrationBuilder.CreateIndex(
                name: "IX_Candidature_Statut",
                schema: "dbo",
                table: "Candidatures",
                column: "Statut");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candidatures",
                schema: "dbo");
        }
    }
}
