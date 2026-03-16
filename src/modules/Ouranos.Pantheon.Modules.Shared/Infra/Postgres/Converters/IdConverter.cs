using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Converters;

public sealed class IdConverter<T>() : ValueConverter<Id<T>, Guid>(
    id => Guid.Parse(id.Value),
    value => new Id<T>(value.ToString())
);