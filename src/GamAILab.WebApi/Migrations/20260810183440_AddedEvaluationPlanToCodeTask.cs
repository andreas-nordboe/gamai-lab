using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedEvaluationPlanToCodeTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AICodeEvaluationPlan",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CodeTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodeTaskVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    Criteria = table.Column<string>(type: "TEXT", nullable: false),
                    CommonMistakes = table.Column<string>(type: "TEXT", nullable: false),
                    FeedbackInstructions = table.Column<string>(type: "TEXT", nullable: false),
                    ModelUsed = table.Column<string>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    PromptVersion = table.Column<string>(type: "TEXT", nullable: false),
                    SchemaVersion = table.Column<string>(type: "TEXT", nullable: false),
                    InitiatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PlanningDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Tests = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AICodeEvaluationPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AICodeEvaluationPlan_CodeTasks_CodeTaskId",
                        column: x => x.CodeTaskId,
                        principalTable: "CodeTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AICodeEvaluationPlan_CodeTaskId",
                table: "AICodeEvaluationPlan",
                column: "CodeTaskId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AICodeEvaluationPlan");
        }
    }
}
