using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Pathfinding;

public class ProcessorFindAStarPath : IProcessor<(Vector3 from, Vector3 to), List<Vector3>>
{
    private readonly AStarPathfinder aStarPathfinder;

    public ProcessorFindAStarPath(AStarPathfinder aStarPathfinder)
    {
        this.aStarPathfinder = aStarPathfinder;
    }

    public List<Vector3> Process((Vector3 from, Vector3 to) positions)
    {
        return aStarPathfinder.FindPath(positions.from, positions.to, 0);
    }
}
