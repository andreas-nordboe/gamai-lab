using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedClassroomSimulationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassroomSimulations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    InitiatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomSimulations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIPersonaSimulations_ClassroomSimulationId",
                table: "AIPersonaSimulations",
                column: "ClassroomSimulationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIPersonaSimulations_ClassroomSimulations_ClassroomSimulationId",
                table: "AIPersonaSimulations",
                column: "ClassroomSimulationId",
                principalTable: "ClassroomSimulations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIPersonaSimulations_ClassroomSimulations_ClassroomSimulationId",
                table: "AIPersonaSimulations");

            migrationBuilder.DropTable(
                name: "ClassroomSimulations");

            migrationBuilder.DropIndex(
                name: "IX_AIPersonaSimulations_ClassroomSimulationId",
                table: "AIPersonaSimulations");
        }
    }
}
