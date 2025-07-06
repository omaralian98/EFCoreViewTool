using System.Reflection;
using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Models;
using EFCoreViewTool.Core.Utilities;
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
        var currentDir = Directory.GetCurrentDirectory();

        string project;
        string startupProject;

        if (request.Project is not null && request.StartupProject is not null)
        {
            // Case 1: both provided
            project = request.Project;
            startupProject = request.StartupProject;
        }
        else if (request.Project is not null || request.StartupProject is not null)
        {
            // Case 2: one provided, use it for both
            var same = request.Project ?? request.StartupProject!;
            project = same;
            startupProject = same;
        }
        else
        {
            // Case 3: neither provided, use current directory
            project = currentDir;
            startupProject = currentDir;
        }
        
        var startupDllPath = projectBuildService.BuildAndGetDllPath(startupProject);
        var startupAssembly = Assembly.LoadFrom(startupDllPath);

        string projectDllPath;
        Assembly projectAssembly;

        if (project == startupProject)
        {
            // same project
            projectDllPath = startupDllPath;
            projectAssembly = startupAssembly;
        }
        else
        {
            // find project output from startup’s bin folder
            var projectName = Path.GetFileName(project.TrimEnd(Path.DirectorySeparatorChar));
            projectDllPath = Path.GetDirectoryName(startupDllPath)?.GetDllOrDefault(projectName)
                             ?? throw new InvalidOperationException($"Project DLL for '{projectName}' not found");

            projectAssembly = Assembly.LoadFrom(projectDllPath);
        }
        
        
        var ctx = new ProjectInfo(
            DepsFile: Path.Combine(Path.GetDirectoryName(startupDllPath)!,
                $"{Path.GetFileNameWithoutExtension(startupDllPath)}.deps.json"),
            AdditionalProbingPath: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages"),
            RuntimeConfig: Path.Combine(Path.GetDirectoryName(startupDllPath)!,
                $"{Path.GetFileNameWithoutExtension(startupDllPath)}.runtimeconfig.json"),
            MigrationName: request.MigrationName,
            Assembly: projectDllPath,
            Project: Path.Combine(project, $"{projectAssembly.GetName().Name}.csproj"),
            StartupAssembly: startupDllPath,
            StartupProject: Path.Combine(startupProject, $"{startupAssembly.GetName().Name}.csproj"),
            ProjectDirectory: project,
            RootNamespace: projectAssembly.GetName().Name!,
            WorkingDirectory: currentDir
        );
        
        await migrationFileWriterService.WriteMigrationsAsync(
            viewDiscoveryService.Discover([projectAssembly, startupAssembly]),
            ctx,
            Path.Combine(project, "Migrations"),
            request.MigrationName,
            $"{ctx.RootNamespace}.Migrations"
        );
    }
}