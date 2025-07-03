using System.Reflection;

namespace EFCoreViewTool.Core.Interfaces;

public interface IProjectLoader
{
    public IEnumerable<Assembly> LoadAssemblies(string? projectPath = null, string? startupProjectPath = null);
}