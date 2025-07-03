using EFCoreViewTool.Core.Enums;
using EFCoreViewTool.Core.Interfaces;

namespace EFCoreViewTool.Core.Services;

public class PostgresMigrationScriptGenerator : IMigrationScriptGenerator
{
    public string GenerateCreateViewScript(string viewName, string viewBodySql, ViewType viewType)
    {
        var viewTypeSql = viewType == ViewType.Materialized ? "MATERIALIZED VIEW" : "VIEW";
        return $"CREATE {viewTypeSql} \"{viewName}\" AS\n{viewBodySql};";
    }

    public string GenerateDropViewScript(string viewName, ViewType viewType)
    {
        var viewTypeSql = viewType == ViewType.Materialized ? "MATERIALIZED VIEW" : "VIEW";
        return $"DROP {viewTypeSql} IF EXISTS \"{viewName}\";";
    }
} 