using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreViewTool.Core.Interfaces;

public interface IMigrationFileWriterService
{
    public Task WriteMigrationsAsync(List<ViewConfiguratorInfo> views, ProjectContextInfo projectContextInfo,
        string destinationDirectory, string className, string namespaceName);
}