using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedHallucinationCheckerEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HallucinationCheckResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AICodeTaskFeedbackId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    ConflictedClaims = table.Column<string>(type: "TEXT", nullable: false),
                    LLMModelUsed = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GenerationTimeInMilliseconds = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallucinationCheckResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HallucinationCheckResults_AICodeTaskFeedbacks_AICodeTaskFeedbackId",
                        column: x => x.AICodeTaskFeedbackId,
                        principalTable: "AICodeTaskFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HallucinationCheckResults_AICodeTaskFeedbackId",
                table: "HallucinationCheckResults",
                column: "AICodeTaskFeedbackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HallucinationCheckResults");
        }
    }
}
