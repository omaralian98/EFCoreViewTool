using EFCoreViewTool.Core.Enums;

namespace EFCoreViewTool.Core.Models;

public class ViewConfiguratorInfo
{
    public Type ConfiguratorType { get; set; }
    public Type DbContextType { get; set; }
    public Type EntityType { get; set; }
    public string ViewName { get; set; }
    public ViewType ViewType { get; set; }
}