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

    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    
    public HashSet<AIPersonaSimulationResponse> SelectedAnalysisSummaries { get; set; } = new();
    public List<AIPersonaSimulationResponse> AnalysisSummary { get; set; } = new();
    private bool _isLoadingSummaries;

    protected override async Task OnInitializedAsync()
    {
        _isLoadingSummaries = true;

        try
        {
            AnalysisSummary = await AIPersonaAnalysisService.GetAIPersonaAnalysisSummaryAsync();
        }
        finally
        {
            _isLoadingSummaries = false;
        }
    }
    
    // Exports selected from UI
    private async Task OnExportAnalysisReportClicked()
    {
        if (_isLoadingSummaries)
        {
            Snackbar.Add("Failed to export analysis data", Severity.Error);
            return;
        }
        
        if (SelectedAnalysisSummaries.Count == 0)
        {
            Snackbar.Add("Failed to export analysis data. Please select at least one analysis below first.", Severity.Error);
            return;
        }
        
        using var streamReference = BuildCsvStructure(SelectedAnalysisSummaries);
        
        await JSRuntime.InvokeVoidAsync("downloadFile", $"GamAILab-Analysis-{DateTime.Now:yyyy-MM-dd}.csv", streamReference);
    }
    
    // Exports the clicked one
    private async Task OnExportSelectedAnalysisReportClicked(AIPersonaSimulationResponse analysisSummary)
    {
        if (_isLoadingSummaries)
        {
            Snackbar.Add("Failed to export analysis data", Severity.Error);
            return;
        }
        
       using var streamReference = BuildCsvStructure(new List<AIPersonaSimulationResponse>
       {
           analysisSummary
       });
        
        await JSRuntime.InvokeVoidAsync("downloadFile", $"GamAILab-Analysis-{DateTime.Now:yyyy-MM-dd}.csv", streamReference);
    }

    private DotNetStreamReference  BuildCsvStructure(IEnumerable<AIPersonaSimulationResponse> analysisSummaries)
    {
        var csvData = new StringBuilder();
        
        // Row headers
        csvData.AppendLine("Simulation Id,AI Personas,Code Task,Total Personas,Successful Personas,Failed Personas,LLM Model,Duration (ms),Started At,Completed At");

        foreach (var analysisSummary in analysisSummaries)
        {
            var combinedPersonaNames = string.Join("; ", analysisSummary.PersonasUsed?.Select(persona => persona.Name) ?? []);
            
            csvData.AppendLine(
                $"{ReplaceBadCharacters(analysisSummary.SimulationId.ToString())}," +
                $"{ReplaceBadCharacters(combinedPersonaNames)}," +
                $"{ReplaceBadCharacters(analysisSummary.CodeTaskTitle)}," +
                $"{analysisSummary.AIPersonaTotalCount}," +
                $"{analysisSummary.SuccessfulPersonasCount}," +
                $"{analysisSummary.FailedPersonasCount}," +
                $"{ReplaceBadCharacters(analysisSummary.LlmModelUsed)}," +
                $"{analysisSummary.DurationInMilliseconds}," +
                $"{ReplaceBadCharacters(analysisSummary.StartedAt.ToString("O"))}," +
                $"{ReplaceBadCharacters(analysisSummary.CompletedAt.ToString("O"))}");
        }
        
        var bytes = Encoding.UTF8.GetBytes(csvData.ToString());
        var stream = new MemoryStream(bytes);

        return new DotNetStreamReference(stream);
    }
    
    private static string ReplaceBadCharacters(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}