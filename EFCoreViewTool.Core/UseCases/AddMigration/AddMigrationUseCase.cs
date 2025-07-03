using System.Reflection;
using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreViewTool.Core.UseCases.AddMigration;

public class AddMigrationUseCase(
    IProjectBuildService projectBuildService,
    IViewDiscoveryService viewDiscoveryService,
    IMigrationFileWriterService migrationFileWriterService,
    ILogger<AddMigrationUseCase> logger)
    : IAddMigrationUseCase
{
    public async Task ExecuteAsync(AddMigrationRequest request)
    {
        request.Project ??= Directory.GetCurrentDirectory();
        request.StartupProject ??= Directory.GetCurrentDirectory();

        Assembly? currentAssembly =
            request.Project == request.StartupProject ? Assembly.GetExecutingAssembly() : null;
        string? currentAssemblyPath = currentAssembly is not null ? Directory.GetCurrentDirectory() : null;

        var projectdllPath = currentAssemblyPath ?? projectBuildService.BuildAndGetDllPath(request.Project);
        var startupProjectdllPath =
            currentAssemblyPath ?? projectBuildService.BuildAndGetDllPath(request.StartupProject);
        var projectAssembly = currentAssembly ?? Assembly.LoadFrom(projectdllPath);
        var startupAssembly = currentAssembly ?? Assembly.LoadFrom(startupProjectdllPath);

        var pro = new ProjectContextInfo()
        {
            ProjectDllPath = projectdllPath,
            StartupDllPath = startupProjectdllPath,
            ProjectAssembly = projectAssembly,
            StartupAssembly = startupAssembly
        };

        var configurator = viewDiscoveryService.Discover([pro.ProjectAssembly, pro.StartupAssembly]);

        foreach (var viewConfiguratorInfo in configurator)
        {
            logger.LogInformation("ViewSqlGenerator.GenerateSql called for {ViewName}", viewConfiguratorInfo.ViewName);
            logger.LogInformation("DbContextType: {DbContextType}", viewConfiguratorInfo.DbContextType.Name);
            logger.LogInformation("ConfiguratorType: {ConfiguratorType}", viewConfiguratorInfo.ConfiguratorType.Name);
            logger.LogInformation("ViewType: {ViewType}", viewConfiguratorInfo.ViewType);
        }

        await migrationFileWriterService.WriteMigrationsAsync(configurator, pro, $"{request.Project}/Migrations", 
            request.MigrationName, $"{pro.ProjectAssembly.GetName().Name}.Migrations");
    }
}