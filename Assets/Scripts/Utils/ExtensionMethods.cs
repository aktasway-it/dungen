using System;
using System.Collections.Generic;
using System.Linq;
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

	public static void Shuffle<TKey, TValue>(this Dictionary<TKey, TValue> source)
	{
        KeyValuePair<TKey, TValue>[] keyValuePairs = source.ToArray();

		for (int i = keyValuePairs.Length - 1; i >= 0; i--)
		{
			int j = RandomUtility.Range(0, i);
			KeyValuePair<TKey, TValue> temp = keyValuePairs[i];
			keyValuePairs[i] = keyValuePairs[j];
			keyValuePairs[j] = temp;
		}

        source = keyValuePairs.ToDictionary(k => k.Key, k => k.Value);
	}
}
