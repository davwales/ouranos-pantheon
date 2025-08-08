using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Infra.Postgres.Converters;

public sealed class IdConverter<T>() : ValueConverter<Id<T>, string>(
    id => id.Value,
    value => new Id<T>(value)
);