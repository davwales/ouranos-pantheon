using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

public static class AssistantToolServiceCollectionExtensions
{
    public static IServiceCollection AddAssistantTool<TContext, TTool>(
        this IServiceCollection services
    )
        where TContext : class
        where TTool : class, IAssistantTool<TContext>
    {
        return services.AddScoped<IAssistantTool<TContext>, TTool>();
    }
}
