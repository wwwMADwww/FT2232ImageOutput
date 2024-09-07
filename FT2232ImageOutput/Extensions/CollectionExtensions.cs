using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace FT2232ImageOutput.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<T> Each<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
        return collection;
    }
}
