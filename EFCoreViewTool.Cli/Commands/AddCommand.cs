using System.Reflection;
using Cocona;
using EFCoreViewTool.Cli.Models;
using EFCoreViewTool.Core.Contracts;
using EFCoreViewTool.Core.Interfaces;
using EFCoreViewTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using EFCoreViewTool.Core.UseCases.AddMigration;
using Microsoft.Extensions.Logging;

namespace EFCoreViewTool.Cli.Commands;

public class AddCommand(
    IAddMigrationUseCase addMigrationUseCase
)
{
    [Command(name: "add")]
    public async Task Add(AddCommandParameters parameters)
    {
        var request = new AddMigrationRequest
        (
            MigrationName: parameters.Name,
            Project: parameters.Project,
            StartupProject: parameters.StartupProject,
            Context: parameters.Context,
            Namespace: parameters.Namespace
        );
        await addMigrationUseCase.ExecuteAsync(request);
    }
}