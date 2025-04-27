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
using GgStatAggregator.Models;
using GgStatAggregator.Components.Pages.StatAggregator;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DB Context
builder.Services.AddDbContext<GgStatAggregatorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Serilog into the .NET ILogger Pipeline
Log.Logger = LoggerUtility.ConfigureLogger(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Host.UseSerilog();

// Add services
builder.Services.AddScoped<IService<Player>, PlayerService>();
builder.Services.AddScoped<IService<StatSet>, StatSetService>();
builder.Services.AddScoped<IService<Table>, TableService>();
builder.Services.AddScoped<StatAggregatorForm>();

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
