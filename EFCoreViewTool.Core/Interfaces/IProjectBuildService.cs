namespace EFCoreViewTool.Core.Interfaces;

public interface IProjectBuildService
{
    public string BuildAndGetDllPath(string projectPath);
}