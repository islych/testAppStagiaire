using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Candidatures.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinataireTransmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinataireTransmission",
                schema: "dbo",
                table: "Candidatures",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinataireTransmission",
                schema: "dbo",
                table: "Candidatures");
        }
    }
}
