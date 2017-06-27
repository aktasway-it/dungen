using System;
using System.Collections.Generic;

using Random = System.Random;

public static class ExtensionMethods
{
    public static T GetRandomElement<T>(this IList<T> list)
    {
        return list[RandomUtility.Range(0, list.Count)];
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = RandomUtility.Range(0, n);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
