using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedHallucinationCheckerEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ConsistencyScore",
                table: "AIHallucinationCheckResults",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCheckedClaims",
                table: "AIHallucinationCheckResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsistencyScore",
                table: "AIHallucinationCheckResults");

            migrationBuilder.DropColumn(
                name: "TotalCheckedClaims",
                table: "AIHallucinationCheckResults");
        }
    }
}
