using System;
using Assets.Scripts.Pathfinding;

public class ProcessorFindClusterPath : IProcessor<ClusterResultWrapper, ClusterResultWrapper>
{
    private readonly Func<ClusterResultWrapper, ClusterResultWrapper> func;

    public ProcessorFindClusterPath(HPAPathfinder hPAPathfinder)
    {
        this.func = hPAPathfinder.FindClusterPath;
    }

    public ClusterResultWrapper Process(ClusterResultWrapper positions)
    {
        return func?.Invoke(positions);
    }
}
