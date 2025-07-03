using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using EFCoreViewTool.Core.Contracts;

namespace EFCoreViewTool.Core.Services;

public class MigrationFileWriterService(
    IDbContextFactoryService dbContextFactoryService,
    IMigrationScriptGeneratorFactory generatorFactory,
    ILogger<MigrationFileWriterService> logger)
    : IMigrationFileWriterService
{
    private readonly Dictionary<Type, (DbContext, IMigrationScriptGenerator)> _cached = [];

    public async Task WriteMigrationsAsync(List<ViewConfiguratorInfo> views, ProjectContextInfo projectContextInfo,
        string destinationDirectory, string className, string namespaceName)
    {
        Directory.CreateDirectory(destinationDirectory);


        var upSqlStatements = new List<string>();
        var downSqlStatements = new List<string>();

        foreach (var view in views)
        {
            (DbContext? db, IMigrationScriptGenerator? gen) existing = _cached.GetValueOrDefault(view.DbContextType);
            
            var context = existing.db ??
                            dbContextFactoryService.CreateDbContext(view.DbContextType, projectContextInfo);
            var generator = existing.gen ?? generatorFactory.Get(context);
            _cached[view.DbContextType] = (context, generator);
            
            var configuratorInstance = Activator.CreateInstance(view.ConfiguratorType);

            var method = view.ConfiguratorType.GetMethod(nameof(IViewConfigurator<DbContext, object>.ConfigureView));

            var query = method?.Invoke(configuratorInstance, [context]) as IQueryable;
            var viewBody = query?.ToQueryString() ?? "";

            var createScript = generator.GenerateCreateViewScript(view.ViewName, viewBody, view.ViewType);
            var dropScript = generator.GenerateDropViewScript(view.ViewName, view.ViewType);
            upSqlStatements.Add(createScript);
            downSqlStatements.Add(dropScript);
        }

        var migrationClass = GenerateMigrationClass(className, namespaceName, upSqlStatements, downSqlStatements);
        
        var migrationName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{className}.cs";
        
        var migrationFilePath = Path.Combine(destinationDirectory, migrationName);
        
        await File.WriteAllTextAsync(migrationFilePath, migrationClass);
        logger.LogInformation("Wrote migration class to {FilePath}", migrationFilePath);
    }

    private static string GenerateMigrationClass(
        string className,
        string namespaceName,
        List<string> upSqlStatements,
        List<string> downSqlStatements)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using Microsoft.EntityFrameworkCore.Migrations;");
        sb.AppendLine();
        sb.AppendLine("#nullable disable");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <inheritdoc />");
        sb.AppendLine($"    public partial class {className} : Migration");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <inheritdoc />");
        sb.AppendLine("        protected override void Up(MigrationBuilder migrationBuilder)");
        sb.AppendLine("        {");

        foreach (var sql in upSqlStatements)
        {
            var escapedSql = sql.Replace("\"", "\"\"");
            sb.AppendLine($"            migrationBuilder.Sql(@\"\n{escapedSql}\");");
        }

        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        /// <inheritdoc />");
        sb.AppendLine("        protected override void Down(MigrationBuilder migrationBuilder)");
        sb.AppendLine("        {");

        foreach (var sql in downSqlStatements)
        {
            var escapedSql = sql.Replace("\"", "\"\"");
            sb.AppendLine($"            migrationBuilder.Sql(@\"{escapedSql}\");");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}