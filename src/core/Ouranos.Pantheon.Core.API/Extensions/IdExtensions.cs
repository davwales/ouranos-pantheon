using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.API.Extensions;

public static class IdExtensions
{
    public static IRequestExecutorBuilder BindModelId<T>(this IRequestExecutorBuilder builder)
    {
        return builder
            .BindRuntimeType(typeof(Id<T>), typeof(StringType))
            .AddTypeConverter<Id<T>, string>(x => x.Value)
            .AddTypeConverter<string, Id<T>>(x => new Id<T>(x));
    }
}