using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.API.Filters;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Shared.API.Extensions;

public static class IdExtensions
{
    public static IRequestExecutorBuilder BindModelId<T>(this IRequestExecutorBuilder builder)
    {
        return builder
            .BindRuntimeType(typeof(Id<T>), typeof(StringType))
            .AddTypeConverter<Id<T>, string>(x => x.ToString())
            .AddTypeConverter<string, Id<T>>(Id<T>.Parse);
    }

    public static IFilterConventionDescriptor BindModelIdFilter<T>(this IFilterConventionDescriptor descriptor)
    {
        return descriptor.BindRuntimeType<Id<T>, IdFilterInputType<T>>();
    }
}