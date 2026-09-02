using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedAnalysisEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AnalysisEvaluationCorrectness",
                table: "AIPersonaSimulations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnalysisFeedbackClarity",
                table: "AIPersonaSimulations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnalysisFeedbackCorrectness",
                table: "AIPersonaSimulations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnalysisFeedbackUsefulness",
                table: "AIPersonaSimulations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AnalysisHallucinationDetected",
                table: "AIPersonaSimulations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisNotes",
                table: "AIPersonaSimulations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisEvaluationCorrectness",
                table: "AIPersonaSimulations");

            migrationBuilder.DropColumn(
                name: "AnalysisFeedbackClarity",
                table: "AIPersonaSimulations");

            migrationBuilder.DropColumn(
                name: "AnalysisFeedbackCorrectness",
                table: "AIPersonaSimulations");

            migrationBuilder.DropColumn(
                name: "AnalysisFeedbackUsefulness",
                table: "AIPersonaSimulations");

            migrationBuilder.DropColumn(
                name: "AnalysisHallucinationDetected",
                table: "AIPersonaSimulations");

            migrationBuilder.DropColumn(
                name: "AnalysisNotes",
                table: "AIPersonaSimulations");
        }
    }
}
