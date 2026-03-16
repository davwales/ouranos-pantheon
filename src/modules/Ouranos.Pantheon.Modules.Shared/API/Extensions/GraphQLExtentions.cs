using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.API.FieldHandlers;
using Ouranos.Pantheon.Modules.Shared.API.Mutations;
using Ouranos.Pantheon.Modules.Shared.API.Queries;

namespace Ouranos.Pantheon.Modules.Shared.API.Extensions;

public static class GraphQlExtentions
{
    public static IRequestExecutorBuilder ConfigureGraphQl(
        this IServiceCollection services,
        IConfiguration configuration,
        IReadOnlyCollection<IPantheonModule> modules
    )
    {
        var builder = services
            .AddGraphQLServer()
            .AddFiltering(descriptor =>
                {
                    descriptor = descriptor.AddDefaults();

                    foreach (var module in modules)
                    {
                        descriptor = module.ConfigureSchemaFilters(descriptor);
                    }
                }
            )
            .AddSorting()
            .AddQueryType<Query>()
            .AddTypeExtensions<Query>()
            .AddMutationType<Mutation>()
            .AddTypeExtensions<Mutation>()
            .AddMutationConventions()
            .BindCommonTypes()
            .AddOuranosConventions()
            .ModifyRequestOptions(o =>
                {
                    var includeExceptionDetails = configuration.GetValue("Ouranos:IncludeExceptionDetails", false);
                    o.IncludeExceptionDetails = includeExceptionDetails;

                    var requestTimeoutSeconds = configuration.GetValue("Ouranos:RequestTimeout", 30);
                    o.ExecutionTimeout = TimeSpan.FromSeconds(requestTimeoutSeconds);
                }
            );

        foreach (var module in modules)
        {
            builder = module.ConfigureSchema(builder);
        }

        return builder;
    }

    private static IRequestExecutorBuilder AddTypeExtensions<T>(this IRequestExecutorBuilder builder)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.SelectMany(x => x.GetTypes());

        var matchingTypes = types.Where(t =>
            t is { IsClass: true, IsSealed: true } &&
            t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute<T>), false).Length != 0
        );

        return matchingTypes.Aggregate(builder, (current, type) => current.AddTypeExtension(type));
    }

    private static IRequestExecutorBuilder BindCommonTypes(this IRequestExecutorBuilder builder)
    {
        return builder.BindRuntimeType(typeof(object), typeof(AnyType));
    }

    private static IRequestExecutorBuilder AddOuranosConventions(this IRequestExecutorBuilder builder)
    {
        return builder
            .AddConvention<IFilterConvention>(
                new FilterConventionExtension(x => x
                    .AddProviderExtension(
                        new QueryableFilterProviderExtension(e => e
                            .AddFieldHandler<QueryableStringInvariantHandler>()
                        )
                    )
                )
            );
    }
}