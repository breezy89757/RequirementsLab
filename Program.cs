using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using RequirementsLab.Components;
using RequirementsLab.Services;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false);

// Register Azure OpenAI ChatClient
var endpoint = builder.Configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Missing AzureOpenAI:Endpoint");
var apiKey = builder.Configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Missing AzureOpenAI:ApiKey");
var deploymentName = builder.Configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";

var azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
IChatClient chatClient = azureClient.GetChatClient(deploymentName).AsIChatClient();

builder.Services.AddSingleton(chatClient);

// Register application services
builder.Services.AddScoped<SolutionGeneratorService>();
builder.Services.AddScoped<FeasibilityEvaluatorService>();
builder.Services.AddScoped<PocTemplateService>();
builder.Services.AddScoped<ImplementationPlanService>();

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
