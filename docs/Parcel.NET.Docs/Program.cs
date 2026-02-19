using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Parcel.NET.Docs;
using Parcel.NET.Docs.Services;
using Parcel.NET.Docs.Playground.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<DocContentService>();
builder.Services.AddScoped<ApiDocService>();

builder.Services.AddScoped<PlaygroundSessionService>();
builder.Services.AddScoped<CarrierClientFactory>();

await builder.Build().RunAsync();
