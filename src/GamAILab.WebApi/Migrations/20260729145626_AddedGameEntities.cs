using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedGameEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LearnerGameProgressId",
                table: "CodeTasks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameObjectives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectiveId = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TargetValue = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentValue = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameObjectives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearnerGameProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Currency = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerGameProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Achievement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    LearnerGameProgressId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Achievement_LearnerGameProgresses_LearnerGameProgressId",
                        column: x => x.LearnerGameProgressId,
                        principalTable: "LearnerGameProgresses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeTasks_LearnerGameProgressId",
                table: "CodeTasks",
                column: "LearnerGameProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_Achievement_LearnerGameProgressId",
                table: "Achievement",
                column: "LearnerGameProgressId");

            migrationBuilder.AddForeignKey(
                name: "FK_CodeTasks_LearnerGameProgresses_LearnerGameProgressId",
                table: "CodeTasks",
                column: "LearnerGameProgressId",
                principalTable: "LearnerGameProgresses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodeTasks_LearnerGameProgresses_LearnerGameProgressId",
                table: "CodeTasks");

            migrationBuilder.DropTable(
                name: "Achievement");

            migrationBuilder.DropTable(
                name: "CustomData");

            migrationBuilder.DropTable(
                name: "GameObjectives");

            migrationBuilder.DropTable(
                name: "LearnerGameProgresses");

            migrationBuilder.DropIndex(
                name: "IX_CodeTasks_LearnerGameProgressId",
                table: "CodeTasks");

            migrationBuilder.DropColumn(
                name: "LearnerGameProgressId",
                table: "CodeTasks");
        }
    }
}
