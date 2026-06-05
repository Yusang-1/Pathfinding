using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> GetDeepCopy<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        var copy = new Dictionary<TKey, TValue>();
        
        foreach(var item in dictionary)
        {
            copy.Add(item.Key, item.Value);
        }

        return copy;
    }
}
