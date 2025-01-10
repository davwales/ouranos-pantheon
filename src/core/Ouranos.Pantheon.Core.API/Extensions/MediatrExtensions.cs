using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.API.Extensions;

public static class MediatrExtensions
{
    public static IServiceCollection AddDefaultMediatrHandlers(this IServiceCollection services)
    {
        return services.AddEntityHandlers(
            typeof(GetAllEntitiesHandler<>),
            typeof(GetEntityHandler<>),
            typeof(DeleteEntityHandler<>)
        );
    }

    private static IServiceCollection AddEntityHandlers(this IServiceCollection services, params Type[] handlerTypes)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.SelectMany(x => x.GetTypes());

        var entityTypes = types.Where(t =>
            t is { IsAbstract: false, IsInterface: false } &&
            t.IsSubclassOfRawGeneric(typeof(BaseEntity<>))
        );

        foreach (var entityType in entityTypes)
        foreach (var handlerType in handlerTypes)
        {
            var genericHandlerType = handlerType.MakeGenericType(entityType);

            var interfaceType = genericHandlerType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            if (interfaceType != null)
            {
                services.AddTransient(interfaceType, genericHandlerType);
            }
        }

        return services;
    }

    private static bool IsSubclassOfRawGeneric(this Type? toCheck, Type generic)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }

            toCheck = toCheck.BaseType;
        }

        return false;
    }
}