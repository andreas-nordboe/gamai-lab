using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AIPersonaDataAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses",
                type: "INTEGER",
                nullable: true);

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
                name: "FK_AIPersonaSimulationResponses_AnalysisSummaries_AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses",
                column: "AIPersonaAnalysisSummaryResponseId",
                principalTable: "AnalysisSummaries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIPersonaSimulationResponses_AnalysisSummaries_AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses");

            migrationBuilder.DropTable(
                name: "AnalysisSummaries");

            migrationBuilder.DropIndex(
                name: "IX_AIPersonaSimulationResponses_AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses");

            migrationBuilder.DropColumn(
                name: "AIPersonaAnalysisSummaryResponseId",
                table: "AIPersonaSimulationResponses");
        }
    }
}
