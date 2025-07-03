using Microsoft.EntityFrameworkCore;

namespace EFCoreViewTool.Core.Interfaces;

public interface IMigrationScriptGeneratorFactory
{
    public IMigrationScriptGenerator Get(DbContext dbContext);
} 