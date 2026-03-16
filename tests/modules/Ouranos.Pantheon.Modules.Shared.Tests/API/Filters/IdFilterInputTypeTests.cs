using HotChocolate.Data.Filters;
using Ouranos.Pantheon.Modules.Shared.API.Filters;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.API.Filters;

public sealed class IdFilterInputTypeTests
{
    [Fact]
    public void ConfigureOperation_ShouldRegisterExpectedOperations()
    {
        // Arrange
        var inputFilter = new IdFilterInputType<Id<TestEntity>>();
        var descriptor = Substitute.For<IFilterInputTypeDescriptor>();

        // Act
        inputFilter.ConfigureOperations(descriptor);

        // Assert
        descriptor.Received(1).Operation(DefaultFilterOperations.Equals);
        descriptor.Received(1).Operation(DefaultFilterOperations.NotEquals);
        descriptor.Received(1).Operation(DefaultFilterOperations.In);
        descriptor.Received(1).Operation(DefaultFilterOperations.NotIn);
        descriptor.Received(1).AllowAnd(false);
        descriptor.Received(1).AllowOr(false);
    }
}