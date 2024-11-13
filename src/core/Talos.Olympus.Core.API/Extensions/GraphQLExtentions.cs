using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Talos.Olympus.Core.API.Mutations;
using Talos.Olympus.Core.API.Queries;

namespace Talos.Olympus.Core.API.Extensions;

public static class GraphQLExtentions
{
    public static IRequestExecutorBuilder ConfigureGraphQL(
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
            .ModifyRequestOptions(o =>
            {
                var IncludeExceptionDetails = configuration.GetValue("Talos:IncludeExceptionDetails", false);
                o.IncludeExceptionDetails = IncludeExceptionDetails;
            });
    }

    private static IRequestExecutorBuilder AddTypeExtensions<T>(this IRequestExecutorBuilder builder)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.SelectMany(x => x.GetTypes());

        var matchingTypes = types.Where(t =>
            t.IsClass &&
            t.IsSealed &&
            t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute<T>), false).Length != 0
        );

        foreach (var type in matchingTypes) builder = builder.AddTypeExtension(type);

        return builder;
    }

    private static IRequestExecutorBuilder BindCommonTypes(this IRequestExecutorBuilder builder)
    {
        return builder.BindRuntimeType(typeof(object), typeof(AnyType));
    }
}