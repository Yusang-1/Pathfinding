using System;
using Assets.Scripts.Pathfinding;

public class ProcessorSmoothClusterPath : IProcessor<ClusterResultWrapper, ClusterResultWrapper>
{
    private readonly Func<ClusterResultWrapper, ClusterResultWrapper> func;

    public ProcessorSmoothClusterPath(ClusterPathSmoother clusterPathSmoother)
    {
        this.func = clusterPathSmoother.SmoothClusterPath;
    }

    public ClusterResultWrapper Process(ClusterResultWrapper input)
    {
        return func?.Invoke(input);
    }
}
