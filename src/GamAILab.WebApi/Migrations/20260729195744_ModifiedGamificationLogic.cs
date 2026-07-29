using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedGamificationLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AchievementId",
                table: "Achievement",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AchievementId",
                table: "Achievement");
        }
    }
}
