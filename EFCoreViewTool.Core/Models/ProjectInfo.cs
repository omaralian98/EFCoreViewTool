using System.Reflection;

namespace EFCoreViewTool.Core.Models;

public record ProjectInfo(
    string DepsFile,
    string AdditionalProbingPath,
    string RuntimeConfig,
    string MigrationName,
    string Assembly,
    string Project,
    string StartupAssembly,
    string StartupProject,
    string ProjectDirectory,
    string RootNamespace,
    string WorkingDirectory
);

