using System.Reflection;
using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Tests.Utils.Extensions;

public static class ObjectExtensions
{
    public static object? Protected(this object substitute, string memberName, params object[] args)
    {
        var method = substitute
            .GetType()
            .GetMethod(memberName, BindingFlags.Instance | BindingFlags.NonPublic);

        if (method is null || !method.IsVirtual)
        {
            throw new Exception("Must be a virtual member");
        }

        return method.Invoke(substitute, args);
    }

    /// <summary>Sets the init-only CreatedAt backing field directly (bypasses EF's change tracker).</summary>
    public static TEntity WithCreatedAt<TEntity>(this TEntity entity, DateTimeOffset createdAt)
        where TEntity : class
    {
        Guard.Against.Null(entity);

        FieldInfo? backingField = null;
        for (var t = typeof(TEntity); t is not null && backingField is null; t = t.BaseType)
        {
            backingField = t.GetField(
                "<CreatedAt>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
        }

        if (backingField is null)
        {
            throw new InvalidOperationException(
                $"{typeof(TEntity).Name} has no CreatedAt backing field."
            );
        }

        backingField.SetValue(entity, createdAt);
        return entity;
    }
}
