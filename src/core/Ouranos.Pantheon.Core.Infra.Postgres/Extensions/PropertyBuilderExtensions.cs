using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Core.Infra.Postgres.Converters;

namespace Ouranos.Pantheon.Core.Infra.Postgres.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<Id<T>> HasIdConversion<T>(
        this PropertyBuilder<Id<T>> builder
    )
    {
        return builder.HasConversion<IdConverter<T>>();
    }
}