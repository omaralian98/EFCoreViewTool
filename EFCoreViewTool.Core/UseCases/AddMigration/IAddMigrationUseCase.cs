using System.Threading.Tasks;

namespace EFCoreViewTool.Core.UseCases.AddMigration;

public interface IAddMigrationUseCase
{
    Task ExecuteAsync(AddMigrationRequest request);
} 