using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;

public sealed class IdConverter<T>()
    : ValueConverter<Id<T>, Guid>(id => Guid.Parse(id.Value), value => new Id<T>(value.ToString()));
