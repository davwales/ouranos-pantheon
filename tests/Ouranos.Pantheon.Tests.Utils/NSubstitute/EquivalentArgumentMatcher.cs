using System.Collections;
using System.Reflection;
using NSubstitute.Core.Arguments;

namespace Ouranos.Pantheon.Tests.Utils.NSubstitute;

public sealed class EquivalentArgumentMatcher<T>(T? expected) : IArgumentMatcher<T>
{
    private readonly HashSet<(object, object)> _comparedReferences = [];

    public bool IsSatisfiedBy(T? argument)
    {
        _comparedReferences.Clear();
        return AreObjectsEqual(expected, argument);
    }

    private bool AreObjectsEqual(object? obj1, object? obj2)
    {
        if (ReferenceEquals(obj1, obj2))
        {
            return true;
        }

        if (obj1 is null || obj2 is null)
        {
            return false;
        }

        if (obj1.GetType() != obj2.GetType())
        {
            return false;
        }

        if (!_comparedReferences.Add((obj1, obj2)))
        {
            return true;
        }

        if (obj1.GetType().IsPrimitive || obj1 is string)
        {
            return obj1.Equals(obj2);
        }

        if (obj1 is IEnumerable enumerable1 && obj2 is IEnumerable enumerable2)
        {
            return AreEnumerablesEqual(enumerable1, enumerable2);
        }

        return AreComplexObjectsEqual(obj1, obj2);
    }

    private bool AreComplexObjectsEqual(object obj1, object obj2)
    {
        var type = obj1.GetType();
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var properties = type.GetProperties(bindingFlags);
        foreach (var prop in properties)
        {
            if (!prop.CanRead)
            {
                continue;
            }

            var value1 = prop.GetValue(obj1);
            var value2 = prop.GetValue(obj2);

            if (!AreObjectsEqual(value1, value2))
            {
                return false;
            }
        }

        var fields = type.GetFields(bindingFlags);
        foreach (var field in fields)
        {
            var value1 = field.GetValue(obj1);
            var value2 = field.GetValue(obj2);

            if (!AreObjectsEqual(value1, value2))
            {
                return false;
            }
        }

        return true;
    }

    private bool AreEnumerablesEqual(IEnumerable enumerable1, IEnumerable enumerable2)
    {
        var list1 = enumerable1.Cast<object>().ToList();
        var list2 = enumerable2.Cast<object>().ToList();

        if (list1.Count != list2.Count)
        {
            return false;
        }

        for (var i = 0; i < list1.Count; i++)
        {
            if (!AreObjectsEqual(list1[i], list2[i]))
            {
                return false;
            }
        }

        return true;
    }
}