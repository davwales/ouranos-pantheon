using AutoFixture.Kernel;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

public sealed class IdSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (
            request is not Type { IsGenericType: true } type
            || type.GetGenericTypeDefinition() != typeof(Id<>)
        )
        {
            return new NoSpecimen();
        }

        var instance = Activator.CreateInstance(type, Guid.NewGuid().ToString());
        if (instance is null)
        {
            return new NoSpecimen();
        }

        return instance;
    }
}
