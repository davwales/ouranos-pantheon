using HotChocolate.Data.Filters;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.API.Filters;

public sealed class IdFilterInputType<T> : FilterInputType, IComparableOperationFilterInputType
{
    protected override void Configure(IFilterInputTypeDescriptor descriptor)
    {
        ConfigureOperations(descriptor);
    }

    public void ConfigureOperations(IFilterInputTypeDescriptor descriptor)
    {
        descriptor
            .Operation(DefaultFilterOperations.Equals)
            .Type(typeof(Id<T>))
            .MakeNullable();

        descriptor
            .Operation(DefaultFilterOperations.NotEquals)
            .Type(typeof(Id<T>))
            .MakeNullable();

        descriptor
            .Operation(DefaultFilterOperations.In)
            .Type(typeof(IEnumerable<Id<T>>))
            .MakeNullable();

        descriptor
            .Operation(DefaultFilterOperations.NotIn)
            .Type(typeof(IEnumerable<Id<T>>))
            .MakeNullable();

        descriptor.AllowAnd(false);
        descriptor.AllowOr(false);
    }
}