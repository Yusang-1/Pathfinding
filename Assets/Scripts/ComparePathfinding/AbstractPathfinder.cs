using UnityEngine;
using System.Collections.Generic;

public abstract class AbstractPathfinder
{
    public abstract List<Vector3> FindPath(Vector3 start, Vector3 destination, float unitRadius);

    protected abstract List<Vector3> CaculateResult(Dictionary<Vector2Int, PathNode> nodes, Vector2Int current, Vector2Int start);

    protected abstract float CaculateHeuristic(Vector2Int from, Vector2Int to);
    

    protected struct PathNode
    {
        public float g;
        public float h;
        public readonly float f => g + h;
        public Vector2Int index;
        public Vector2Int parentIndex;
        public Vector2Int beforeNodeIndex;
        public bool isParentSet;
    }
}
