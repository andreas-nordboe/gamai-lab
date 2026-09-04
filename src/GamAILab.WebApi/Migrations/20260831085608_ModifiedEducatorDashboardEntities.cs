using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedEducatorDashboardEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EngagementDropRiskLevel",
                table: "AIPersonaSimulationResult",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EngagementIsDeclining",
                table: "AIPersonaSimulationResult",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PassedLatestCodeTask",
                table: "AIPersonaSimulationResult",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PredictedEngagementScore",
                table: "AIPersonaSimulationResult",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngagementDropRiskLevel",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropColumn(
                name: "EngagementIsDeclining",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropColumn(
                name: "PassedLatestCodeTask",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropColumn(
                name: "PredictedEngagementScore",
                table: "AIPersonaSimulationResult");
        }
    }
}
