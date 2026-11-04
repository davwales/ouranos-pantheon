using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;

public static class PublicModeEndpointExtensions
{
    /// <summary>Rejects every request to the endpoint with 403 while public mode is on.</summary>
    public static RouteHandlerBuilder DisabledInPublicMode(this RouteHandlerBuilder builder)
    {
        return builder.WithPublicModeFilter(new PublicModeWriteFilter(_ => true));
    }

    /// <summary>
    /// Rejects a request with 403 while public mode is on when its <typeparamref name="TInput"/>
    /// argument satisfies <paramref name="writes"/>; other requests run as usual.
    /// </summary>
    public static RouteHandlerBuilder DisabledInPublicMode<TInput>(
        this RouteHandlerBuilder builder,
        Func<TInput, bool> writes
    )
    {
        return builder.WithPublicModeFilter(
            new PublicModeWriteFilter(context => context.Arguments.OfType<TInput>().Any(writes))
        );
    }

    private static RouteHandlerBuilder WithPublicModeFilter(
        this RouteHandlerBuilder builder,
        PublicModeWriteFilter filter
    )
    {
        return builder
            .AddEndpointFilter(filter)
            .WithMetadata(new PublicModeWriteMetadata())
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
