using System.Reflection;

namespace EFCoreViewTool.Core.Models;

public class ProjectContextInfo
{
    public string ProjectDllPath { get; set; }
    public string StartupDllPath { get; set; }
    public Assembly ProjectAssembly { get; set; }
    public Assembly StartupAssembly { get; set; }
}