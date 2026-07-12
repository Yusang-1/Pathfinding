using System.Collections.Generic;
using UnityEngine;

public class ListPool<T, t> where T : List<t>, new()
{
    private static readonly Stack<T> unusedPool = new();
    private static readonly HashSet<T> inUse = new();

    private const int MaxPoolSize = 64;

    public static T GetValue(int capacity = -1)
    {
        T value;
        if (unusedPool.Count > 0)
        {
            value = unusedPool.Pop();
        }
        else
        {
            value = new T();
        }

        if (capacity >= 0)
        {
            value.Capacity = capacity;
        }

        inUse.Add(value);
        return value;
    }

    public static void ReleaseValue(T value)
    {
        if (value == null || !inUse.Remove(value))
        {
            return;
        }

        value.Clear();

        if (unusedPool.Count < MaxPoolSize)
        {
            unusedPool.Push(value);
        }
    }
}

public class Vector2IntListPool : ListPool<List<Vector2Int>, Vector2Int> { }

public class Vector3ListPool : ListPool<List<Vector3>, Vector3> { }
