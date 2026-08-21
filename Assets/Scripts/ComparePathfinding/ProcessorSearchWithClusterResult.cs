using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Pathfinding;

public class ProcessorSearchWithClusterResult : IProcessor<ClusterResultWrapper, List<Vector3>>
{
    private readonly Func<ClusterResultWrapper, List<Vector3>> func;

    public ProcessorSearchWithClusterResult(Func<ClusterResultWrapper, List<Vector3>> func)
    {
        this.func = func;
    }

    public List<Vector3> Process(ClusterResultWrapper path)
    {
        return func?.Invoke(path);
    }
}
