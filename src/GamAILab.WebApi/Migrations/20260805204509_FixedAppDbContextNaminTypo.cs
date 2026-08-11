using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class FixedAppDbContextNaminTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AIPeronas",
                table: "AIPeronas");

            migrationBuilder.RenameTable(
                name: "AIPeronas",
                newName: "AIPersonas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AIPersonas",
                table: "AIPersonas",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AIPersonas",
                table: "AIPersonas");

            migrationBuilder.RenameTable(
                name: "AIPersonas",
                newName: "AIPeronas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AIPeronas",
                table: "AIPeronas",
                column: "Id");
        }
    }
}
