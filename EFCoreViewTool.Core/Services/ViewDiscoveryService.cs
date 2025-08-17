using System.Reflection;
using EFCoreViewTool.Core.Attributes;
using EFCoreViewTool.Core.Contracts;
using EFCoreViewTool.Core.Enums;
using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Models;

namespace EFCoreViewTool.Core.Services;

public class ViewDiscoveryService : IViewDiscoveryService
{
    public List<ViewConfiguratorInfo> Discover(List<Assembly> assemblies)
    {
        var configurators = new List<ViewConfiguratorInfo>();
        var configuratorInterfaceType = typeof(IViewConfigurator<,>);
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var configuratorTypes = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false })
                    .Where(t => ImplementsViewConfigurator(t, configuratorInterfaceType))
                    .ToList();
                
                
                foreach (var configType in configuratorTypes)
                {
                    try
                    {
                        var iface = GetViewConfiguratorInterface(configType, configuratorInterfaceType);
                        if (iface == null) continue;
                        
                        var dbContextType = iface.GetGenericArguments()[0];
                        var entityType = iface.GetGenericArguments()[1];

                        var viewTypeAttribute = configType.GetCustomAttribute<ViewTypeAttribute>();
                        var viewType = viewTypeAttribute?.Type ?? ViewType.Standard; 
                        
                        configurators.Add(new ViewConfiguratorInfo
                        {
                            ConfiguratorType = configType,
                            DbContextType = dbContextType,
                            EntityType = entityType,
                            ViewName = entityType.Name,
                            ViewType = viewType
                        });
                        
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
            catch (ReflectionTypeLoadException rtle)
            {
                if (rtle.Types != null)
                {
                    var loadedTypes = rtle.Types.Where(t => t != null).ToArray();
                    
                    var configuratorTypes = loadedTypes
                        .Where(t => t is { IsClass: true } and { IsAbstract: false, IsInterface: false })
                        .Where(t => t != null && ImplementsViewConfigurator(t, configuratorInterfaceType))
                        .ToList();
                    
                    
                    foreach (var configType in configuratorTypes)
                    {
                        try
                        {
                            var iface = GetViewConfiguratorInterface(configType, configuratorInterfaceType);
                            if (iface == null) continue;
                            
                            var dbContextType = iface.GetGenericArguments()[0];
                            var entityType = iface.GetGenericArguments()[1];
                            
                            configurators.Add(new ViewConfiguratorInfo
                            {
                                ConfiguratorType = configType,
                                DbContextType = dbContextType,
                                EntityType = entityType,
                                ViewName = entityType.Name,
                            });
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                continue;
            }
        }
        
        return configurators;
    }
    
    
    private static bool ImplementsViewConfigurator(Type type, Type configuratorInterfaceType)
    {
        try
        {
            return type.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == configuratorInterfaceType);
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private static Type? GetViewConfiguratorInterface(Type type, Type configuratorInterfaceType)
    {
        try
        {
            return type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == configuratorInterfaceType);
        }
        catch (Exception)
        {
            return null;
        }
    }
}