namespace EFCoreViewTool.Core.UseCases.AddMigration;

public class AddMigrationRequest
{
    public required string MigrationName { get; set; }
    public string? Project { get; set; }
    public string? StartupProject { get; set; }
    public string Context { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
} 