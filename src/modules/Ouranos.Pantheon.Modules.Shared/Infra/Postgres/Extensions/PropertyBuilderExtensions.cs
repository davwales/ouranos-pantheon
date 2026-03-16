using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Converters;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<Id<T>> HasIdConversion<T>(
        this PropertyBuilder<Id<T>> builder
    )
    {
        return builder.HasConversion<IdConverter<T>>();
    }
}