using EFCoreViewTool.Cli.Attributes;

namespace EFCoreViewTool.Cli.Models;

using Cocona;


public class AddCommandParameters : ICommandParameterSet
{
    [Argument(Name = "name", Description = "The name of the migration")]
    public required string Name { get; set; }

    [Option('p', Description = "The project to use. Defaults to the current working directory.")]
    [HasDefaultValue]
    [PathExists]
    public string? Project { get; set; }

    [Option('s', Description = "The startup project to use. Defaults to the current working directory.")]
    [HasDefaultValue]
    [PathExists]
    public string? StartupProject { get; set; }

    [Option('c', Description = "The DbContext to use. \"*\" can be used to run the command for all contexts found. This will also disable service discovery through the startup project if a corresponding IDesignTimeDbContextFactory implementation is found.\n")]
    [HasDefaultValue]
    public string Context { get; set; } = "";

    [Option('n', Description = "The namespace to use. Matches the directory by default.")]
    [HasDefaultValue]
    public string Namespace { get; set; } = "";
}