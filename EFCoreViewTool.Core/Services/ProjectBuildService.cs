using System.Diagnostics;
using System.Xml.Linq;
using EFCoreViewTool.Core.Interfaces;

namespace EFCoreViewTool.Core.Services;

public class ProjectBuildService : IProjectBuildService
{
    public string BuildAndGetDllPath(string projectPath)
    {
        if (!Directory.Exists(projectPath))
        {
            throw new DirectoryNotFoundException($"Project folder not found: {projectPath}");
        }

        var csprojFile = Directory.GetFiles(projectPath, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (csprojFile is null)
        {
            throw new FileNotFoundException("No .csproj file found in the specified folder.");
        }
        
        string targetFramework = GetTargetFramework(csprojFile);
        if (string.IsNullOrEmpty(targetFramework))
        {
            throw new Exception("Could not determine target framework from the project file.");
        }
        
        
        var buildProcess = new Process();
        buildProcess.StartInfo.FileName = "dotnet";
        buildProcess.StartInfo.Arguments = $"build \"{csprojFile}\" -c Debug --nologo";
        buildProcess.StartInfo.RedirectStandardOutput = true;
        buildProcess.StartInfo.RedirectStandardError = true;
        buildProcess.StartInfo.UseShellExecute = false;
        buildProcess.StartInfo.CreateNoWindow = true;

        buildProcess.Start();

        buildProcess.WaitForExit();

        if (buildProcess.ExitCode != 0)
        {
            string output = buildProcess.StandardOutput.ReadToEnd();
            string errors = buildProcess.StandardError.ReadToEnd();
            throw new Exception($"Build failed:\n{errors}\n{output}");
        }

        var fileName = Path.GetFileNameWithoutExtension(csprojFile);
        var dllPath = Path.Combine(projectPath, "bin", "Debug", targetFramework, $"{fileName}.dll");
        
        if (File.Exists(dllPath) == false)
        {
            throw new FileNotFoundException($"DLL not found after build: {dllPath}");
        }

        return dllPath;
    }

    private static string GetTargetFramework(string csprojPath)
    {
        var doc = XDocument.Load(csprojPath);
        var tf = doc.Descendants("TargetFramework").FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(tf) == false)
        {
            return tf;
        }
        
        var tfs = doc.Descendants("TargetFrameworks").FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(tfs) == false)
        {
            return tfs.Split(';', ',').First().Trim();
        }

        throw new InvalidOperationException("Could not determine TargetFramework from project file.");
    }
}