using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Trisecmed.Web;
using Trisecmed.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, TrisecmedAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
