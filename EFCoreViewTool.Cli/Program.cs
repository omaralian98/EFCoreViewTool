using Cocona;
using EFCoreViewTool.Cli.Commands;
using EFCoreViewTool.Cli.Filters;
using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Services;
using EFCoreViewTool.Core.UseCases.AddMigration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var builder = CoconaApp.CreateBuilder(args);

// Add logging with filtering to only show your own logs
builder.Services.AddLogging();
// builder.Services.AddLogging(logging =>
// {
//     logging.ClearProviders();
//     logging.AddSimpleConsole(options =>
//     {
//         options.SingleLine = true;
//         options.TimestampFormat = "hh:mm:ss ";
//     });
//     logging.SetMinimumLevel(LogLevel.Information);
//     logging.AddFilter((category, level) =>
//     {
//         // Only log your own namespace, or filter out Microsoft/EFCore/System logs
//         return !category.StartsWith("Microsoft") && !category.StartsWith("System") && !category.StartsWith("EFCore");
//     });
// });

// Register services
builder.Services.AddScoped<IProjectBuildService, ProjectBuildService>();
builder.Services.AddScoped<IViewDiscoveryService, ViewDiscoveryService>();
builder.Services.AddScoped<IProjectLoader, ProjectLoader>();
builder.Services.AddScoped<IDbContextFactoryService, DbContextFactoryService>();
builder.Services.AddScoped<IAddMigrationUseCase, AddMigrationUseCase>();
builder.Services.AddScoped<IMigrationFileWriterService, MigrationFileWriterService>();
builder.Services.AddSingleton<SqlServerMigrationScriptGenerator>();
builder.Services.AddSingleton<PostgresMigrationScriptGenerator>();
builder.Services.AddSingleton<IMigrationScriptGeneratorFactory, MigrationScriptGeneratorFactory>();
var app = builder.Build();

app.UseFilter(new CommandExceptionsFilter());

app.AddCommands<AddCommand>();

app.Run();
