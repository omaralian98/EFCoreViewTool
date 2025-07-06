namespace EFCoreViewTool.Core.UseCases.AddMigration;

public record AddMigrationRequest(
    string MigrationName,
    string? Project,
    string? StartupProject,
    string Context,
    string Namespace
);