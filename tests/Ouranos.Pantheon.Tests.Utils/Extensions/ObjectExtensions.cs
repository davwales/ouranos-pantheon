using System.Reflection;

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
}