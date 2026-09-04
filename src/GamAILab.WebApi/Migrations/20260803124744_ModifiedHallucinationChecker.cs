using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedHallucinationChecker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HallucinationCheckResults_AICodeTaskFeedbacks_AICodeTaskFeedbackId",
                table: "HallucinationCheckResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HallucinationCheckResults",
                table: "HallucinationCheckResults");

            migrationBuilder.RenameTable(
                name: "HallucinationCheckResults",
                newName: "AIHallucinationCheckResults");

            migrationBuilder.RenameIndex(
                name: "IX_HallucinationCheckResults_AICodeTaskFeedbackId",
                table: "AIHallucinationCheckResults",
                newName: "IX_AIHallucinationCheckResults_AICodeTaskFeedbackId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AIHallucinationCheckResults",
                table: "AIHallucinationCheckResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIHallucinationCheckResults_AICodeTaskFeedbacks_AICodeTaskFeedbackId",
                table: "AIHallucinationCheckResults",
                column: "AICodeTaskFeedbackId",
                principalTable: "AICodeTaskFeedbacks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIHallucinationCheckResults_AICodeTaskFeedbacks_AICodeTaskFeedbackId",
                table: "AIHallucinationCheckResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AIHallucinationCheckResults",
                table: "AIHallucinationCheckResults");

            migrationBuilder.RenameTable(
                name: "AIHallucinationCheckResults",
                newName: "HallucinationCheckResults");

            migrationBuilder.RenameIndex(
                name: "IX_AIHallucinationCheckResults_AICodeTaskFeedbackId",
                table: "HallucinationCheckResults",
                newName: "IX_HallucinationCheckResults_AICodeTaskFeedbackId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HallucinationCheckResults",
                table: "HallucinationCheckResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HallucinationCheckResults_AICodeTaskFeedbacks_AICodeTaskFeedbackId",
                table: "HallucinationCheckResults",
                column: "AICodeTaskFeedbackId",
                principalTable: "AICodeTaskFeedbacks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
