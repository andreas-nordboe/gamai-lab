using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;

namespace GamAILab.Frontend.Client.Components.CodeTasks;

public partial class CodeEditorPanel : ComponentBase
{
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
            Theme =  "vs-dark"
        };
    }
}