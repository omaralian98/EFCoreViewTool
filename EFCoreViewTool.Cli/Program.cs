using Cocona;
using EFCoreViewTool.Cli.Commands;
using EFCoreViewTool.Cli.Filters;
using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Services;
using Microsoft.Extensions.DependencyInjection;

var debugArgs = new[] { "add", "testt", "-p", "/home/mr-sure21/RiderProjects/Tele-Shop/Infrastructure", "-s", "/home/mr-sure21/RiderProjects/Tele-Shop/API" };

var builder = CoconaApp.CreateBuilder(debugArgs);

// Add logging
builder.Services.AddLogging();

// Register services
builder.Services.AddScoped<IProjectBuildService, ProjectBuildService>();
builder.Services.AddScoped<IViewDiscoveryService, ViewDiscoveryService>();
builder.Services.AddScoped<IProjectLoader, ProjectLoader>();
builder.Services.AddScoped<IDbContextFactoryService, DbContextFactoryService>();
var app = builder.Build();

app.UseFilter(new CommandExceptionsFilter());

app.AddCommands<AddCommand>();

app.Run();
