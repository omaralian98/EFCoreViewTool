using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreViewTool.Core.Interfaces;

public interface IDbContextFactoryService
{
    public DbContext CreateDbContext(Type dbContextType, ProjectContextInfo context, string? dbContextName = null);
}