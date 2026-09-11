using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Pathfinding;

public class ListPool<T, t> where T : List<t>, new()
{
    private static readonly Stack<T> unusedPool = new();

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

        return value;
    }

    public static void ReleaseValue(T value)
    {
        if (value == null)
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

public class Pool<T> where T : class, IPoolObject, new()
{
    protected static readonly Stack<T> unusedPool = new();

    protected const int MaxPoolSize = 32;

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

        return value;
    }

    public static void ReleaseValue(T value)
    {
        if (value == null)
        {
            return;
        }

        (value as IPoolObject).Clear();

        if (unusedPool.Count < MaxPoolSize)
        {
            unusedPool.Push(value);
        }
    }
}

public interface IPoolObject
{
    void Clear();
}

public class ClusterSmootherResultPool : Pool<ClusterSmootherResult>
{
    public static ClusterSmootherResult GetValue(List<Vector2Int> clusters, Vector2Int exitIndex, Vector2Int startIndex)
    {
        ClusterSmootherResult value;
        if (unusedPool.Count > 0)
        {
            value = unusedPool.Pop();
            value.SetData(clusters, exitIndex, startIndex);
        }
        else
        {
            value = new ClusterSmootherResult();
            value.SetData(clusters, exitIndex, startIndex);
        }

        return value;
    }
}

// public class ClusterResultPool : Pool<ClusterResult> { }
