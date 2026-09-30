using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Pathfinding;

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

        // (value as IPoolObject).Clear();

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

public class ClusterResultWrapperPool : Pool<ClusterResultWrapper>
{
    public static void ClearReleaseValue(ClusterResultWrapper wrapper)
    {
        wrapper.ResetAll();
        ReleaseValue(wrapper);
    }
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

public class GraphNodePool : Pool<HPAGraph.GraphNode>
{
    public static HPAGraph.GraphNode GetValue(Vector2Int position, Vector2Int direction)
    {
        HPAGraph.GraphNode value;
        if (unusedPool.Count > 0)
        {
            value = unusedPool.Pop();
            value.SetData(position, direction);
        }
        else
        {
            value = new HPAGraph.GraphNode();
            value.SetData(position, direction);
        }

        return value;
    }
}
