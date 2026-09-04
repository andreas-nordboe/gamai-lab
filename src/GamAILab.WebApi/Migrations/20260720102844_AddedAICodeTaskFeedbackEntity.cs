using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedAICodeTaskFeedbackEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AICodeTaskFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodeSubmissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskOutcome = table.Column<int>(type: "INTEGER", nullable: false),
                    HintMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CodeTaskExecutionEvidence = table.Column<string>(type: "TEXT", nullable: false),
                    LLMModelUsed = table.Column<string>(type: "TEXT", nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GeneationTimeInMs = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AICodeTaskFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AICodeTaskFeedbacks_CodeSubmissions_CodeSubmissionId",
                        column: x => x.CodeSubmissionId,
                        principalTable: "CodeSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AICodeTaskFeedbacks_CodeSubmissionId",
                table: "AICodeTaskFeedbacks",
                column: "CodeSubmissionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AICodeTaskFeedbacks");
        }
    }
}
