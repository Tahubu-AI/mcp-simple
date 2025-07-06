using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using System.Net.Http.Headers;
using System.Reflection;
using mcp_simple_server;
using ModelContextProtocol.Server;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Get API key from environment variable, fallback to DEMO_KEY for development
var nasaApiKey = Environment.GetEnvironmentVariable("NASA_API_KEY") ?? "DEMO_KEY";

builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient() { BaseAddress = new Uri("https://api.nasa.gov/") };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("mars-photos-tool", "1.0"));
    return client;
});

builder.Services.AddSingleton<MarsPhotosTools>();
builder.Services.AddSingleton(new NasaApiConfiguration { ApiKey = nasaApiKey });

var app = builder.Build();

// Ensure tools are instantiated before MCP server starts
var marsTools = app.Services.GetRequiredService<MarsPhotosTools>();
Console.Error.WriteLine($"Tool instance created: {marsTools != null}");

await app.RunAsync();