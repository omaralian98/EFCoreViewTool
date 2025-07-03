using System.Reflection;
using Cocona;
using EFCoreViewTool.Cli.Models;
using EFCoreViewTool.Core.Contracts;
using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreViewTool.Cli.Commands;

public class AddCommand(
    IProjectBuildService projectBuildService,
    IViewDiscoveryService viewDiscoveryService,
    IProjectLoader projectLoader,
    IDbContextFactoryService dbContextFactoryService
)
{
    [Command(name: "add")]
    public async Task Add(AddCommandParameters parameters)
    {
        parameters.Project ??= Directory.GetCurrentDirectory();
        parameters.StartupProject ??= Directory.GetCurrentDirectory();

        Assembly? currentAssembly =
            parameters.Project == parameters.StartupProject ? Assembly.GetExecutingAssembly() : null;
        string? currentAssemblyPath = currentAssembly is not null ? Directory.GetCurrentDirectory() : null;

        var projectdllPath = currentAssemblyPath ?? projectBuildService.BuildAndGetDllPath(parameters.Project);
        var startupProjectdllPath =
            currentAssemblyPath ?? projectBuildService.BuildAndGetDllPath(parameters.StartupProject);
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
            Console.WriteLine($"[DEBUG] ViewSqlGenerator.GenerateSql called for {viewConfiguratorInfo.ViewName}");
            Console.WriteLine($"[DEBUG] DbContextType: {viewConfiguratorInfo.DbContextType.Name}");
            Console.WriteLine($"[DEBUG] ConfiguratorType: {viewConfiguratorInfo.ConfiguratorType.Name}");
            Console.WriteLine($"[DEBUG] ViewType: {viewConfiguratorInfo.ViewType}");

            var top = dbContextFactoryService.CreateDbContext(viewConfiguratorInfo.DbContextType, pro);

            var configuratorInstance = Activator.CreateInstance(viewConfiguratorInfo.ConfiguratorType);

            var method = viewConfiguratorInfo.ConfiguratorType.GetMethod(nameof(IViewConfigurator<DbContext, object>.ConfigureView));

            var result = method?.Invoke(configuratorInstance, [top]) as IQueryable;

            Console.WriteLine(result?.ToQueryString());

        }
    }
}