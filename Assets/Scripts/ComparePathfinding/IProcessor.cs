using System;
using System.Collections.Generic;
using UnityEngine;

public interface IProcessor<in TIn, out TOut>
{
    TOut Process(TIn input);
}

public delegate TOut ProcessorDelegate<in TIn, out TOut>(TIn input);

public class Combined<A, B, C> : IProcessor<A, C>
{
    private readonly IProcessor<A, B> first;
    private readonly IProcessor<B, C> second;

    public Combined(IProcessor<A, B> first, IProcessor<B, C> second)
    {
        this.first = first;
        this.second = second;
    }

    public C Process(A input) => second.Process(first.Process(input));
}

public class FindAStarPathProcessor : IProcessor<(Vector3 from, Vector3 to), List<Vector3>>
{
    private readonly AStarPathfinder aStarPathfinder;

    public FindAStarPathProcessor(AStarPathfinder aStarPathfinder)
    {
        this.aStarPathfinder = aStarPathfinder;
    }

    public List<Vector3> Process((Vector3 from, Vector3 to) positions)
    {
        return aStarPathfinder.FindPath(positions.from, positions.to, 0);
    }
}

public class FindClusterPathProcessor : IProcessor<ClusterResultWrapper, ClusterResultWrapper>
{
    private readonly Func<ClusterResultWrapper, ClusterResultWrapper> func;

    public FindClusterPathProcessor(HPAPathfinder hPAPathfinder)
    {
        this.func = hPAPathfinder.FindClusterPath;
    }

    public ClusterResultWrapper Process(ClusterResultWrapper positions)
    {
        return func?.Invoke(positions);
    }
}

public class SmoothClusterPathProcessor : IProcessor<ClusterResultWrapper, ClusterResultWrapper>
{
    private readonly Func<ClusterResultWrapper, ClusterResultWrapper> func;

    public SmoothClusterPathProcessor(ClusterPathSmoother clusterPathSmoother)
    {
        this.func = clusterPathSmoother.SmoothClusterPath;
    }

    public ClusterResultWrapper Process(ClusterResultWrapper input)
    {
        return func?.Invoke(input);
    }
}

public class SearchWithClusterResultProcessor : IProcessor<ClusterResultWrapper, List<Vector3>>
{
    private readonly Func<ClusterResultWrapper, List<Vector3>> func;

    public SearchWithClusterResultProcessor(Func<ClusterResultWrapper, List<Vector3>> func)
    {
        this.func = func;
    }

    public List<Vector3> Process(ClusterResultWrapper path)
    {
        return func?.Invoke(path);
    }
}
