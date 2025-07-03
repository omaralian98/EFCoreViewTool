using EFCoreViewTool.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFCoreViewTool.Core.Services;

public class MigrationScriptGeneratorFactory(
    SqlServerMigrationScriptGenerator sqlServerGenerator,
    PostgresMigrationScriptGenerator postgresGenerator)
    : IMigrationScriptGeneratorFactory
{
    public IMigrationScriptGenerator Get(DbContext dbContext)
    {
        var providerName = dbContext.Database.ProviderName;
        return providerName switch
        {
            "Microsoft.EntityFrameworkCore.SqlServer" => sqlServerGenerator,
            "Npgsql.EntityFrameworkCore.PostgreSQL" => postgresGenerator,
            _ => throw new NotSupportedException($"Provider not supported: {providerName}")
        };
    }
} 