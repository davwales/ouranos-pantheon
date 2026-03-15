using AutoFixture.Kernel;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

public sealed class IdSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is not Type type)
        {
            return new NoSpecimen();
        }

        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Id<>))
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