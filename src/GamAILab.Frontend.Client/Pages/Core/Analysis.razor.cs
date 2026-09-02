using System.Globalization;
using System.Text;
using GamAILab.Frontend.Client.Services;
using GamAILab.Shared.Models.AIPersonaSimulation.DTOs;
using GamAILab.Shared.Models.Analysis;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class Analysis : ComponentBase
{
     [Inject]
    public IAnalysisService AIPersonaAnalysisService { get; set; } = null!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = null!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    public List<ClassroomSimulation> ClassroomSimulations { get; set; } = [];

    private bool _isLoadingSummaries;

    protected override async Task OnInitializedAsync()
    {
        _isLoadingSummaries = true;

        try
        {
            ClassroomSimulations =
                await AIPersonaAnalysisService.ListClassroomSimulationsAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add(
                $"Failed to load classroom simulations: {ex.Message}",
                Severity.Error);
        }
        finally
        {
            _isLoadingSummaries = false;
        }
    }

    private async Task OnExportAnalysisReportClicked()
    {
        var stream = BuildCsvStructure(ClassroomSimulations);

        stream.Position = 0;

        using var streamReference = new DotNetStreamReference(stream);

        await JSRuntime.InvokeVoidAsync("downloadFile", "classroom-simulation-analysis.csv", streamReference);
    }

    private MemoryStream BuildCsvStructure(List<ClassroomSimulation> simulations)
    {
        var csv = new StringBuilder();

        csv.AppendLine("Simulation Id,Persona,Simulated Minute,Attempt,Engagement Score,Struggles,Learning Outcomes,Task Outcome");

        foreach (var simulation in simulations)
        {
            foreach (var response in simulation.SimulationResponses)
            {
                foreach (var result in response.PersonaResults)
                {
                    csv.AppendLine(string.Join(",",
                        Csv(simulation.Id.ToString()),
                        Csv(result.Persona?.Name ?? ""),
                        response.SimulatedMinute,
                        response.AttemptNumber,
                        result.EngagementScore,
                        Csv(string.Join("; ", result.Struggles ?? [])),
                        Csv(string.Join("; ", result.LearningOutcomes ?? [])),
                        Csv(result.SubmissionResult?.AIFeedback?.TaskOutcome.ToString() ?? "")
                    ));
                }
            }
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());

        return new MemoryStream(bytes);
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}