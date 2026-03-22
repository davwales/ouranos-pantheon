using System.Runtime.CompilerServices;

namespace Ouranos.Pantheon.Modules.Shared.Domain;

public sealed class NavigationPropertyNotLoadedException<TEntity>(
    [CallerMemberName] string propertyName = ""
) : InvalidOperationException(
    $"Navigation property '{propertyName}' on '{typeof(TEntity).Name}' was not loaded. Use Include() to eagerly load it."
);
