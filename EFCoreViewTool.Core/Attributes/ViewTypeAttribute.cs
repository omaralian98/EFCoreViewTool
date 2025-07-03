using EFCoreViewTool.Core.Enums;

namespace EFCoreViewTool.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ViewTypeAttribute(ViewType type) : Attribute
{
    public ViewType Type { get; } = type;
}