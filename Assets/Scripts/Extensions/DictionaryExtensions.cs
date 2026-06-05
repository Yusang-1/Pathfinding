using System;
using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> GetDeepCopy<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<TValue, TValue> cloneValue)
    {
        var copy = new Dictionary<TKey, TValue>(dictionary.Count);

        foreach (var item in dictionary)
        {
            copy.Add(item.Key, cloneValue(item.Value));
        }

        return copy;
    }
}
