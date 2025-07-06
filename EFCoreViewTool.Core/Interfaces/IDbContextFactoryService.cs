using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreViewTool.Core.Interfaces;

public interface IDbContextFactoryService
{
    public DbContext CreateDbContext(Type dbContextType, ProjectInfo context, string? dbContextName = null);
}