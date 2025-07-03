using EFCoreViewTool.Core.Interfaces;

namespace EFCoreViewTool.Core.Services;

using System.Reflection;


public class ProjectLoader : IProjectLoader
{
    public IEnumerable<Assembly> LoadAssemblies(string? projectPath = null, string? startupProjectPath = null)
    {
        var assemblies = new List<Assembly>();
        
        // Load assemblies from project path if provided
        if (!string.IsNullOrEmpty(projectPath))
        {
            var projectAssemblies = LoadAssembliesFromPath(projectPath);
            assemblies.AddRange(projectAssemblies);
        }
        
        // Load assemblies from startup project path if provided
        if (!string.IsNullOrEmpty(startupProjectPath))
        {
            var startupAssemblies = LoadAssembliesFromPath(startupProjectPath);
            assemblies.AddRange(startupAssemblies);
        }
        
        // If no specific paths provided, try current directory
        if (string.IsNullOrEmpty(projectPath) && string.IsNullOrEmpty(startupProjectPath))
        {
            var currentDirectoryAssemblies = LoadAssembliesFromPath(Directory.GetCurrentDirectory());
            assemblies.AddRange(currentDirectoryAssemblies);
        }
        
        // If no assemblies found, throw exception
        if (!assemblies.Any())
        {
            throw new Exception();
        }
        
        return assemblies.Distinct();
    }
    
    private IEnumerable<Assembly> LoadAssembliesFromPath(string path)
    {
        var assemblies = new List<Assembly>();
        
        if (!Directory.Exists(path))
        {
            return assemblies;
        }
        
        try
        {
            // Look for .dll files in the path
            var dllFiles = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
            
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    // Get the assembly name without extension
                    var assemblyName = Path.GetFileNameWithoutExtension(dllFile);
                    if (string.IsNullOrEmpty(assemblyName))
                        continue;
                        
                    Console.WriteLine($"[DEBUG] Trying to load assembly: {assemblyName}");
                    
                    // Try to load by name first (like EF Core does)
                    var assembly = Assembly.Load(assemblyName);
                    if (!assemblies.Contains(assembly))
                    {
                        assemblies.Add(assembly);
                        Console.WriteLine($"[DEBUG] Successfully loaded assembly: {assemblyName}");
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"[DEBUG] Assembly not found in GAC: {Path.GetFileNameWithoutExtension(dllFile)}");
                    // Try LoadFrom as fallback
                    try
                    {
                        var assembly = Assembly.LoadFrom(dllFile);
                        assemblies.Add(assembly);
                        Console.WriteLine($"[DEBUG] Successfully loaded assembly using LoadFrom: {Path.GetFileName(dllFile)}");
                    }
                    catch (BadImageFormatException)
                    {
                        Console.WriteLine($"[DEBUG] Skipping non-.NET assembly: {Path.GetFileName(dllFile)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Failed to load assembly {Path.GetFileName(dllFile)}: {ex.Message}");
                    }
                }
                catch (BadImageFormatException)
                {
                    Console.WriteLine($"[DEBUG] Skipping non-.NET assembly: {Path.GetFileName(dllFile)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Failed to load assembly {Path.GetFileName(dllFile)}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Error processing path {path}: {ex.Message}");
        }
        
        return assemblies;
    }
} 