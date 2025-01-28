using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.API.FieldHandlers;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.API.Queries;

namespace Ouranos.Pantheon.Core.API.Extensions;

public static class GraphQlExtentions
{
    public static IRequestExecutorBuilder ConfigureGraphQl(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddGraphQLServer()
            .AddFiltering()
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
            });
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
            .AddConvention<IFilterConvention>(new FilterConventionExtension(x => x
                .AddProviderExtension(new QueryableFilterProviderExtension(e => e
                    .AddFieldHandler<QueryableStringInvariantHandler>()
                ))
            ));
    }
}