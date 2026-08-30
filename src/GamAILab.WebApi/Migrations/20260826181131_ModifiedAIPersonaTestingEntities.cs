using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedAIPersonaTestingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeAttempt",
                table: "AIPersonaSimulationResult");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassroomSimulationId",
                table: "AIPersonaSimulations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SimulatedMinute",
                table: "AIPersonaSimulations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SimulationTimeStepIndex",
                table: "AIPersonaSimulations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassroomSessionId",
                table: "AIPersonaSimulationResult",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "CodeAttemptId",
                table: "AIPersonaSimulationResult",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EngagementScore",
                table: "AIPersonaSimulationResult",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LearningOutcomes",
                table: "AIPersonaSimulationResult",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Struggles",
                table: "AIPersonaSimulationResult",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "AIPersonaCodeResponse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Struggles = table.Column<string>(type: "TEXT", nullable: false),
                    LearningOutcomes = table.Column<string>(type: "TEXT", nullable: false),
                    EngagementScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIPersonaCodeResponse", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIPersonaSimulationResult_CodeAttemptId",
                table: "AIPersonaSimulationResult",
                column: "CodeAttemptId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIPersonaSimulationResult_AIPersonaCodeResponse_CodeAttemptId",
                table: "AIPersonaSimulationResult",
                column: "CodeAttemptId",
                principalTable: "AIPersonaCodeResponse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIPersonaSimulationResult_AIPersonaCodeResponse_CodeAttemptId",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropTable(
                name: "AIPersonaCodeResponse");

            migrationBuilder.DropIndex(
                name: "IX_AIPersonaSimulationResult_CodeAttemptId",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropColumn(
                name: "ClassroomSimulationId",
                table: "AIPersonaSimulations");

            migrationBuilder.DropColumn(
                name: "SimulatedMinute",
                table: "AIPersonaSimulations");

            migrationBuilder.DropColumn(
                name: "SimulationTimeStepIndex",
                table: "AIPersonaSimulations");

            migrationBuilder.DropColumn(
                name: "ClassroomSessionId",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropColumn(
                name: "CodeAttemptId",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropColumn(
                name: "EngagementScore",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropColumn(
                name: "LearningOutcomes",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropColumn(
                name: "Struggles",
                table: "AIPersonaSimulationResult");

            migrationBuilder.AddColumn<string>(
                name: "CodeAttempt",
                table: "AIPersonaSimulationResult",
                type: "TEXT",
                nullable: true);
        }
    }
}
