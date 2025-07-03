using System.Reflection;
using EFCoreViewTool.Core.Models;

namespace EFCoreViewTool.Core.Interfaces;

public interface IViewDiscoveryService
{
    public IEnumerable<ViewConfiguratorInfo> Discover(List<Assembly> assemblies);
}