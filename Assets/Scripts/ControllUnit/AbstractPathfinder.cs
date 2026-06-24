using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public abstract class AbstractPathfinder
    {
        public abstract List<Vector3> FindPath(Vector3 start, Vector3 destination, out PathResult result, float unitRadius);

        protected abstract List<Vector3> CaculateResult(Dictionary<Vector2Int, PathNode> nodes, Vector2Int current, Vector2Int start);

        protected abstract float CaculateHeuristic(Vector2Int from, Vector2Int to);

        protected abstract List<Vector2Int> GetNeighborNode(Vector2Int current, float unitRadius);

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
}
