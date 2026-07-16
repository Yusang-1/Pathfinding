using System.Collections.Generic;
using System.Linq;
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

public class Pool<T> where T : class, new()
{
    private static readonly Stack<T> unusedPool = new();
    private static readonly HashSet<T> inUse = new();

    private const int MaxPoolSize = 64;
    private const int InitSize = 10;
    
    public static void Initialize()
    {
        for(int i = 0; i < InitSize; i++)
        {
            T value = new();
            unusedPool.Push(value);
        }
    }
    
    public static T GetValue()
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

        inUse.Add(value);
        return value;
    }

    public static void ReleaseValue(T value)
    {
        if (value == null || !inUse.Remove(value))
        {
            return;
        }

        (value as IPoolObject).Clear();

        if (unusedPool.Count < MaxPoolSize)
        {
            unusedPool.Push(value);
        }
    }
    
    public static void ReleaseAllValue()
    {
        var CopyOfInUse = inUse.ToArray();
        foreach(var item in CopyOfInUse)
        {
            ReleaseValue(item);
        }
    }
}

public interface IPoolObject
{
    void Clear();
}

public class ClusterResultPool : Pool<ClusterResult> { }
