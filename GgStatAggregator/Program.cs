using MudBlazor.Services;
using GgStatAggregator.Components;
using GgStatAggregator.Data;
using Microsoft.EntityFrameworkCore;
using GgStatAggregator.Services;
using Serilog;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Events;
using GgStatAggregator.Logger;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Set up environment aware config
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.json{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add DB Context
builder.Services.AddDbContext<GgStatAggregatorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Serilog into the .NET ILogger Pipeline
Log.Logger = LoggerUtility.ConfigureLogger(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Host.UseSerilog();

// Add services
builder.Services.AddScoped(typeof(IPlayerService), typeof(PlayerService));
builder.Services.AddScoped(typeof(IStatSetService), typeof(StatSetService));
builder.Services.AddScoped(typeof(ITableService), typeof(TableService));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
