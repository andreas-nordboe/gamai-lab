using Blazored.LocalStorage;
using GamAILab.Frontend.Components;
using GamAILab.Frontend.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using AuthenticationService = GamAILab.Frontend.Services.AuthenticationService;
using IAuthenticationService = GamAILab.Frontend.Services.IAuthenticationService;

namespace GamAILab.Frontend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        builder.Services.AddBlazoredLocalStorage();
        
        builder.Services.AddMudServices();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "jwt";
            options.DefaultChallengeScheme = "jwt";
        }).AddScheme<AuthenticationSchemeOptions, NoActionAuthenticationHandler>("jwt", null);
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<JWTAuthorisationMessageHandler>(); 
        builder.Services.AddScoped<JWTAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JWTAuthenticationStateProvider>());
        
        builder.Services.AddHttpClient("GamAILabAPI",
            client => client.BaseAddress = new Uri(builder.Configuration.GetConnectionString("GamAILabAPI") ?? throw new InvalidOperationException("GamAILab base url is missing!"))).AddHttpMessageHandler<JWTAuthorisationMessageHandler>();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}