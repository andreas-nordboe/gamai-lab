using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamAILab.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedAISimulationResponseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AIPersonaSimulationResponseId",
                table: "AIPersonas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AICodeTaskFeedbackDTO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                    table.PrimaryKey("PK_AICodeTaskFeedbackDTO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIPersonaSimulationResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExecutionTimeInMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    SimulationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CodeTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodeTaskTitle = table.Column<string>(type: "TEXT", nullable: false),
                    LlmModelUsed = table.Column<string>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIPersonaSimulationResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodeExecutionResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DidComplete = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimedOut = table.Column<bool>(type: "INTEGER", nullable: false),
                    EveryTestPassed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExitCode = table.Column<int>(type: "INTEGER", nullable: false),
                    StandardOutput = table.Column<string>(type: "TEXT", nullable: false),
                    StandardError = table.Column<string>(type: "TEXT", nullable: false),
                    FatalError = table.Column<string>(type: "TEXT", nullable: false),
                    ExecutionDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeExecutionResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearnerGameProgressRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Currency = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerGameProgressRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodeTestResult",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Passed = table.Column<bool>(type: "INTEGER", nullable: false),
                    Error = table.Column<string>(type: "TEXT", nullable: true),
                    CodeExecutionResultId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeTestResult", x => x.Key);
                    table.ForeignKey(
                        name: "FK_CodeTestResult_CodeExecutionResult_CodeExecutionResultId",
                        column: x => x.CodeExecutionResultId,
                        principalTable: "CodeExecutionResult",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AchievementRequest",
                columns: table => new
                {
                    AchievementId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    LearnerGameProgressRequestId = table.Column<int>(type: "INTEGER", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "CodeSubmissionResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubmissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CodeTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodeExecutionId = table.Column<int>(type: "INTEGER", nullable: false),
                    AIFeedbackId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExecutionDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    SubmittedCode = table.Column<string>(type: "TEXT", nullable: false),
                    HallucinationCheckId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameProgressId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeSubmissionResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeSubmissionResult_AICodeTaskFeedbackDTO_AIFeedbackId",
                        column: x => x.AIFeedbackId,
                        principalTable: "AICodeTaskFeedbackDTO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodeSubmissionResult_AIHallucinationCheckResults_HallucinationCheckId",
                        column: x => x.HallucinationCheckId,
                        principalTable: "AIHallucinationCheckResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodeSubmissionResult_CodeExecutionResult_CodeExecutionId",
                        column: x => x.CodeExecutionId,
                        principalTable: "CodeExecutionResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodeSubmissionResult_CodeTasks_CodeTaskId",
                        column: x => x.CodeTaskId,
                        principalTable: "CodeTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodeSubmissionResult_LearnerGameProgressRequest_GameProgressId",
                        column: x => x.GameProgressId,
                        principalTable: "LearnerGameProgressRequest",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AIPersonaSimulationResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodeAttempt = table.Column<string>(type: "TEXT", nullable: true),
                    SubmissionResultId = table.Column<int>(type: "INTEGER", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    AIPersonaSimulationResponseId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIPersonaSimulationResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIPersonaSimulationResult_AIPersonaSimulationResponses_AIPersonaSimulationResponseId",
                        column: x => x.AIPersonaSimulationResponseId,
                        principalTable: "AIPersonaSimulationResponses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AIPersonaSimulationResult_AIPersonas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "AIPersonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIPersonaSimulationResult_CodeSubmissionResult_SubmissionResultId",
                        column: x => x.SubmissionResultId,
                        principalTable: "CodeSubmissionResult",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIPersonas_AIPersonaSimulationResponseId",
                table: "AIPersonas",
                column: "AIPersonaSimulationResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementRequest_LearnerGameProgressRequestId",
                table: "AchievementRequest",
                column: "LearnerGameProgressRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AIPersonaSimulationResult_AIPersonaSimulationResponseId",
                table: "AIPersonaSimulationResult",
                column: "AIPersonaSimulationResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_AIPersonaSimulationResult_PersonaId",
                table: "AIPersonaSimulationResult",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_AIPersonaSimulationResult_SubmissionResultId",
                table: "AIPersonaSimulationResult",
                column: "SubmissionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSubmissionResult_AIFeedbackId",
                table: "CodeSubmissionResult",
                column: "AIFeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSubmissionResult_CodeExecutionId",
                table: "CodeSubmissionResult",
                column: "CodeExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSubmissionResult_CodeTaskId",
                table: "CodeSubmissionResult",
                column: "CodeTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSubmissionResult_GameProgressId",
                table: "CodeSubmissionResult",
                column: "GameProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSubmissionResult_HallucinationCheckId",
                table: "CodeSubmissionResult",
                column: "HallucinationCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeTestResult_CodeExecutionResultId",
                table: "CodeTestResult",
                column: "CodeExecutionResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIPersonas_AIPersonaSimulationResponses_AIPersonaSimulationResponseId",
                table: "AIPersonas",
                column: "AIPersonaSimulationResponseId",
                principalTable: "AIPersonaSimulationResponses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIPersonas_AIPersonaSimulationResponses_AIPersonaSimulationResponseId",
                table: "AIPersonas");

            migrationBuilder.DropTable(
                name: "AchievementRequest");

            migrationBuilder.DropTable(
                name: "AIPersonaSimulationResult");

            migrationBuilder.DropTable(
                name: "CodeTestResult");

            migrationBuilder.DropTable(
                name: "AIPersonaSimulationResponses");

            migrationBuilder.DropTable(
                name: "CodeSubmissionResult");

            migrationBuilder.DropTable(
                name: "AICodeTaskFeedbackDTO");

            migrationBuilder.DropTable(
                name: "CodeExecutionResult");

            migrationBuilder.DropTable(
                name: "LearnerGameProgressRequest");

            migrationBuilder.DropIndex(
                name: "IX_AIPersonas_AIPersonaSimulationResponseId",
                table: "AIPersonas");

            migrationBuilder.DropColumn(
                name: "AIPersonaSimulationResponseId",
                table: "AIPersonas");
        }
    }
}
