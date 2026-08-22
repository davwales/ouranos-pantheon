using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<Id<T>> HasIdConversion<T>(this PropertyBuilder<Id<T>> builder)
    {
        return builder.HasConversion<IdConverter<T>>();
    }
}
