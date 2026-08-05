using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GamAILab.Frontend.Client;
using GamAILab.Frontend.Client.Handlers;
using GamAILab.Frontend.Client.Providers;
using GamAILab.Frontend.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration.GetConnectionString("GamAILabAPI") ?? throw new InvalidOperationException("GamAILAB API connection is missing");

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICodeSubmissionService, CodeSubmissionService>();
builder.Services.AddScoped<ICodeTasksService, CodeTasksService>();
builder.Services.AddScoped<IAIPersonaSimulationService, AIPersonaSimulationService>();

builder.Services.AddScoped<JWTAuthenticationStateProvider>();
builder.Services.AddTransient<AuthenticationStateProvider>(provider => provider.GetRequiredService<JWTAuthenticationStateProvider>());
builder.Services.AddTransient<JwtHandler>();

builder.Services.AddHttpClient("GamAILabAPI", client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<JwtHandler>();

builder.Services.AddScoped(provider =>
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("GamAILabAPI");
});

await builder.Build().RunAsync();