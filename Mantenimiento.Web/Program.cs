using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mantenimiento.Web;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Mantenimiento.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped(sp => (CustomAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5284/") });
builder.Services.AddScoped<OrdenTrabajoClient>();
builder.Services.AddScoped<DependenciaClient>();
builder.Services.AddScoped<UsuarioClient>();
builder.Services.AddScoped<MaterialClient>();
builder.Services.AddScoped<EncuestaClient>();
builder.Services.AddScoped<ProgramadorClient>();
builder.Services.AddScoped<ConfiguracionClient>();
builder.Services.AddScoped<CorreoQueueClient>();
builder.Services.AddScoped<MetricasClient>();

await builder.Build().RunAsync();
