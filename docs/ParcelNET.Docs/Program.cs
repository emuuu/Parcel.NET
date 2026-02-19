using ParcelNET.Docs;
using ParcelNET.Docs.Services;
using ParcelNET.Docs.Playground.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<DocContentService>();
builder.Services.AddScoped<ApiDocService>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<PlaygroundSessionService>();
builder.Services.AddScoped<CarrierClientFactory>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
