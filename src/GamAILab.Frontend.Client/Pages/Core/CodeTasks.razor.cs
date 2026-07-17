using Microsoft.AspNetCore.Components;

namespace GamAILab.Frontend.Client.Pages.Core;

public partial class CodeTasks : ComponentBase
{
   [Inject] public NavigationManager NavigationManager { get; set; }
   
   private void RunCode()
   {
      // TODO   
   }
   
   private void ResetCode()
   {
      // TODO   
   }
   
   private void GetCodeHint()
   {
      // TODO   
   }
   
   private void SubmitCode()
   {
      // TODO   
      // This is just for testing the layout temporarily (a modal might be more appropriate later) 
      NavigationManager.NavigateTo("code-output");
   }
   
   
}