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

public class FindClusterPathProcessor : IProcessor<(Vector3 from, Vector3 to), List<ClusterResult>>
{
    private readonly Func<Vector3, Vector3, float, List<ClusterResult>> func;
    private readonly float unitRadius;

    public FindClusterPathProcessor(HPAPathfinder hPAPathfinder, float unitRadius)
    {
        this.func = hPAPathfinder.FindClusterPath;
        this.unitRadius = unitRadius;
    }

    public List<ClusterResult> Process((Vector3 from, Vector3 to) positions)
    {
        return func?.Invoke(positions.from, positions.to, unitRadius);
    }
}

public class SmoothClusterPathProcessor : IProcessor<List<ClusterResult>, List<ClusterResult>>
{
    private readonly Func<List<ClusterResult>, float, List<ClusterResult>> func;
    private readonly float unitRadius;

    public SmoothClusterPathProcessor(ClusterPathSmoother clusterPathSmoother, float unitRadius)
    {
        this.func = clusterPathSmoother.SmoothClusterPath;
        this.unitRadius = unitRadius;
    }

    public List<ClusterResult> Process(List<ClusterResult> input)
    {
        return func?.Invoke(input, unitRadius);
    }
}

public class SearchWithClusterResultProcessor : IProcessor<List<ClusterResult>, List<Vector3>>
{
    private readonly Func<List<ClusterResult>, List<Vector3>> func;

    public SearchWithClusterResultProcessor(Func<List<ClusterResult>, List<Vector3>> func)
    {
        this.func = func;
    }

    public List<Vector3> Process(List<ClusterResult> path)
    {
        return func?.Invoke(path);
    }
}
