using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIPersonas_AIPersonaSimulationResponses_AIPersonaSimulationResponseId",
                table: "AIPersonas");

            migrationBuilder.DropForeignKey(
                name: "FK_AIPersonaSimulationResponses_AnalysisSummaries_AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_AIPersonaSimulationResult_AIPersonaSimulationResponses_AIPersonaSimulationResponseId",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropForeignKey(
                name: "FK_CodeSubmissionResult_CodeExecutionResult_CodeExecutionId",
                table: "CodeSubmissionResult");

            migrationBuilder.DropForeignKey(
                name: "FK_CodeTestResult_CodeExecutionResult_CodeExecutionResultId",
                table: "CodeTestResult");

            migrationBuilder.DropTable(
                name: "AnalysisSummaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CodeExecutionResult",
                table: "CodeExecutionResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AIPersonaSimulationResponses",
                table: "AIPersonaSimulationResponses");

            migrationBuilder.DropIndex(
                name: "IX_AIPersonaSimulationResponses_AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses");

            migrationBuilder.DropColumn(
                name: "AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses");

            migrationBuilder.RenameTable(
                name: "CodeExecutionResult",
                newName: "CodeExecutions");

            migrationBuilder.RenameTable(
                name: "AIPersonaSimulationResponses",
                newName: "AIPersonaSimulations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CodeExecutions",
                table: "CodeExecutions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AIPersonaSimulations",
                table: "AIPersonaSimulations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIPersonas_AIPersonaSimulations_AIPersonaSimulationResponseId",
                table: "AIPersonas",
                column: "AIPersonaSimulationResponseId",
                principalTable: "AIPersonaSimulations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIPersonaSimulationResult_AIPersonaSimulations_AIPersonaSimulationResponseId",
                table: "AIPersonaSimulationResult",
                column: "AIPersonaSimulationResponseId",
                principalTable: "AIPersonaSimulations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CodeSubmissionResult_CodeExecutions_CodeExecutionId",
                table: "CodeSubmissionResult",
                column: "CodeExecutionId",
                principalTable: "CodeExecutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CodeTestResult_CodeExecutions_CodeExecutionResultId",
                table: "CodeTestResult",
                column: "CodeExecutionResultId",
                principalTable: "CodeExecutions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIPersonas_AIPersonaSimulations_AIPersonaSimulationResponseId",
                table: "AIPersonas");

            migrationBuilder.DropForeignKey(
                name: "FK_AIPersonaSimulationResult_AIPersonaSimulations_AIPersonaSimulationResponseId",
                table: "AIPersonaSimulationResult");

            migrationBuilder.DropForeignKey(
                name: "FK_CodeSubmissionResult_CodeExecutions_CodeExecutionId",
                table: "CodeSubmissionResult");

            migrationBuilder.DropForeignKey(
                name: "FK_CodeTestResult_CodeExecutions_CodeExecutionResultId",
                table: "CodeTestResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CodeExecutions",
                table: "CodeExecutions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AIPersonaSimulations",
                table: "AIPersonaSimulations");

            migrationBuilder.RenameTable(
                name: "CodeExecutions",
                newName: "CodeExecutionResult");

            migrationBuilder.RenameTable(
                name: "AIPersonaSimulations",
                newName: "AIPersonaSimulationResponses");

            migrationBuilder.AddColumn<int>(
                name: "AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CodeExecutionResult",
                table: "CodeExecutionResult",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AIPersonaSimulationResponses",
                table: "AIPersonaSimulationResponses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AnalysisSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisSummaries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIPersonaSimulationResponses_AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses",
                column: "AIPersonaAnalysisSummaryResponseId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIPersonas_AIPersonaSimulationResponses_AIPersonaSimulationResponseId",
                table: "AIPersonas",
                column: "AIPersonaSimulationResponseId",
                principalTable: "AIPersonaSimulationResponses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIPersonaSimulationResponses_AnalysisSummaries_AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses",
                column: "AIPersonaAnalysisSummaryResponseId",
                principalTable: "AnalysisSummaries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIPersonaSimulationResult_AIPersonaSimulationResponses_AIPersonaSimulationResponseId",
                table: "AIPersonaSimulationResult",
                column: "AIPersonaSimulationResponseId",
                principalTable: "AIPersonaSimulationResponses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CodeSubmissionResult_CodeExecutionResult_CodeExecutionId",
                table: "CodeSubmissionResult",
                column: "CodeExecutionId",
                principalTable: "CodeExecutionResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CodeTestResult_CodeExecutionResult_CodeExecutionResultId",
                table: "CodeTestResult",
                column: "CodeExecutionResultId",
                principalTable: "CodeExecutionResult",
                principalColumn: "Id");
        }
    }
}
