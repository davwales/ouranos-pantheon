using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Infra.Postgres.Converters;

public sealed class IdConverter<T>() : ValueConverter<Id<T>, Guid>(
    id => Guid.Parse(id.Value),
    value => new Id<T>(value.ToString())
);