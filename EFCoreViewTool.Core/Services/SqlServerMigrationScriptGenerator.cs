using EFCoreViewTool.Core.Enums;
using EFCoreViewTool.Core.Interfaces;

namespace EFCoreViewTool.Core.Services;

public class SqlServerMigrationScriptGenerator : IMigrationScriptGenerator
{
    public string GenerateCreateViewScript(string viewName, string viewBodySql, ViewType viewType)
    {
        // TODO: Implement SQL Server-specific script generation
        return $"-- SQL Server CREATE VIEW for {viewName}";
    }

    public string GenerateDropViewScript(string viewName, ViewType viewType)
    {
        // TODO: Implement SQL Server-specific drop script
        return $"-- SQL Server DROP VIEW for {viewName}";
    }
} 