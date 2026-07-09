using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public abstract class AbstractPathfinder
    {
        protected abstract List<Vector3> SearchPath(Vector3 from, Vector3 to, float unitRadius, Func<Vector2Int, float, List<Vector2Int>> getNeighborNodeFunc);
        
        protected abstract float CaculateHeuristic(Vector2Int from, Vector2Int to);
    }
    
    public struct PathNode
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
