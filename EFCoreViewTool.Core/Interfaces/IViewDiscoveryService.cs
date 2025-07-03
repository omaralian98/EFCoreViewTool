using System.Reflection;
using EFCoreViewTool.Core.Models;

namespace EFCoreViewTool.Core.Interfaces;

public interface IViewDiscoveryService
{
    public List<ViewConfiguratorInfo> Discover(List<Assembly> assemblies);
}