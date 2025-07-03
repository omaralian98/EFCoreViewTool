using Cocona;
using Cocona.Command;
using Cocona.Command.Binder;
using Cocona.Filters;

namespace EFCoreViewTool.Cli.Filters;

public class CommandExceptionsFilter : CommandFilterAttribute
{
    public override async ValueTask<int> OnCommandExecutionAsync(CoconaCommandExecutingContext ctx,
        CommandExecutionDelegate next)
    {
        try
        {
            return await next(ctx);
        }
        catch (Exception ex)
            when (ex is not CoconaException &&
                  ex is not ParameterBinderException &&
                  ex is not CommandNotFoundException &&
                  ex is not CommandExitedException)
        {
            Console.WriteLine($"An error has occurred: {ex}");
        }

        return -1;
    }
}