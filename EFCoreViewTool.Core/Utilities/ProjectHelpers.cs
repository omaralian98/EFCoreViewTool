namespace EFCoreViewTool.Core.Utilities;

public static class ProjectHelpers
{
    public static string? GetProjectOrDefault(this string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return Directory.GetFiles(path, "*.csproj", searchOption).FirstOrDefault();
    }
    
    
    public static string? GetDllOrDefault(this string path, string dllName = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return Directory.GetFiles(path, $"{dllName}.dll", searchOption).FirstOrDefault();
    }
}