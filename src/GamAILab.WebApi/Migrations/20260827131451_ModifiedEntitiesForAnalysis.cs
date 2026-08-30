using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedEntitiesForAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodeSubmissionResult_LearnerGameProgressRequest_GameProgressId",
                table: "CodeSubmissionResult");

            migrationBuilder.DropTable(
                name: "AchievementRequest");

            migrationBuilder.DropTable(
                name: "LearnerGameProgressRequest");

            migrationBuilder.DropIndex(
                name: "IX_CodeSubmissionResult_GameProgressId",
                table: "CodeSubmissionResult");

            migrationBuilder.DropColumn(
                name: "GameProgressId",
                table: "CodeSubmissionResult");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameProgressId",
                table: "CodeSubmissionResult",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LearnerGameProgressRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Currency = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerGameProgressRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AchievementRequest",
                columns: table => new
                {
                    AchievementId = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    LearnerGameProgressRequestId = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementRequest", x => x.AchievementId);
                    table.ForeignKey(
                        name: "FK_AchievementRequest_LearnerGameProgressRequest_LearnerGameProgressRequestId",
                        column: x => x.LearnerGameProgressRequestId,
                        principalTable: "LearnerGameProgressRequest",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeSubmissionResult_GameProgressId",
                table: "CodeSubmissionResult",
                column: "GameProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementRequest_LearnerGameProgressRequestId",
                table: "AchievementRequest",
                column: "LearnerGameProgressRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_CodeSubmissionResult_LearnerGameProgressRequest_GameProgressId",
                table: "CodeSubmissionResult",
                column: "GameProgressId",
                principalTable: "LearnerGameProgressRequest",
                principalColumn: "Id");
        }
    }
}
