using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;

/// <summary>
/// Rejects a request with 403 while public mode is on and <paramref name="writes"/> says the
/// request changes stored data.
/// </summary>
public sealed class PublicModeWriteFilter(Func<EndpointFilterInvocationContext, bool> writes)
    : IEndpointFilter
{
    public const string Detail = "Hermes is in public mode, so changes are disabled.";

    private readonly Func<EndpointFilterInvocationContext, bool> _writes = Guard.Against.Null(
        writes
    );

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        if (_writes(context))
        {
            var flagsmith =
                context.HttpContext.RequestServices.GetRequiredService<IFlagsmithClient>();
            var flags = await flagsmith.GetEnvironmentFlags();

            if (await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: Detail
                );
            }
        }

        return await next(context);
    }
}
