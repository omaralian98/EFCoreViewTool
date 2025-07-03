using EFCoreViewTool.Core.Enums;

namespace EFCoreViewTool.Core.Interfaces;

public interface IMigrationScriptGenerator
{
    public string GenerateCreateViewScript(string viewName, string viewBodySql, ViewType viewType);
    public string GenerateDropViewScript(string viewName, ViewType viewType);
}