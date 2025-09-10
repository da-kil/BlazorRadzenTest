using Microsoft.Extensions.DependencyInjection;

namespace ti8m.BeachBreak.Application.Command.Commands;

internal sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var commandHandlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType().UnderlyingSystemType, typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(commandHandlerType);

        return await (Task<TResponse>)commandHandlerType
            .GetMethod(nameof(ICommandHandler<ICommand<TResponse>, TResponse>.HandleAsync))
            !.Invoke(handler, [command, cancellationToken])!;
    }
}
