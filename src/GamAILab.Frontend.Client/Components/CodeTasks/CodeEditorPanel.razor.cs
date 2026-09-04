using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;

namespace GamAILab.Frontend.Client.Components.CodeTasks;

public partial class CodeEditorPanel : ComponentBase
{
    private StandaloneCodeEditor? _codeEditor;
    
    [Parameter] 
    public string EditorId { get; set; } = Guid.NewGuid().ToString();
    [Parameter] 
    public bool ReadOnly { get; set; }
    [Parameter] 
    public string DefaultCode { get; set; } = string.Empty;
    [Parameter] 
    public string CssClass { get; set; } = "monaco-editor"; // in App.css

    public async Task<string> GetCodeAsync()
    {
        if (_codeEditor is null)
        {
            throw new  InvalidOperationException("CodeEditor has not been initalised yet");
        }

        return await _codeEditor.GetValue();
    }
    
    public async Task SetCodeAsync(string code)
    {
        if (_codeEditor is null)
        {
            throw new  InvalidOperationException("CodeEditor has not been initalised yet");
        }

        await _codeEditor.SetValue(code);
    }
    
    private void EditorDidChangeCursorPosition(CursorPositionChangedEvent eventArgs)
    {
        Console.WriteLine("EditorDidChangeCursorPosition");
    }

    private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            AutomaticLayout = true,
            Language = "python", // TODO hard-coded for now
            Theme =  "vs-dark",
            Value = DefaultCode,
            ReadOnly = ReadOnly
        };
    }
}