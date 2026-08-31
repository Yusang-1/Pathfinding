using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public class ClusterResultWrapper
    {
        public Vector3 From { get; private set; }
        public Vector3 To { get; private set; }
        public float UnitRadius { get; private set; }

        public List<ClusterSmootherResult> ClusterSmootherResult { get; private set; } = new();
        public List<ClusterResult> NewClusterResults { get; private set; } = new();

        public void SetStart(Vector3 from, Vector3 to, float unitRadius)
        {
            this.From = from;
            this.To = to;
            this.UnitRadius = unitRadius;
        }

        public void Reset()
        {

        }

        public void SetClusterResult(List<ClusterResult> results)
        {
            NewClusterResults = results;
        }
        public void SetClusterResult(ClusterResult result)
        {
            NewClusterResults.Add(result);
        }

        public void SetClusterSmootherResult(List<ClusterSmootherResult> smootherResults)
        {
            ClusterSmootherResult = smootherResults;
        }
        public void SetClusterSmootherResult(ClusterSmootherResult smootherResult)
        {
            ClusterSmootherResult.Add(smootherResult);
        }
    }

    public class ClusterSmootherResult
    {
        public List<Vector2Int> ClusterIndexes = new();
        public Vector2Int EnterNodeIndex;
        public Vector2Int ExitNodeIndex;

        public void SetSmootherResult(List<Vector2Int> clusters, Vector2Int exitIndex, Vector2Int startIndex, Vector2Int notIncludeClusterIndex, bool useNotIncludeClusterIndex)
        {
            ClusterIndexes.Clear();
            for (int i = 0; i < clusters.Count; i++)
            {
                if (useNotIncludeClusterIndex && clusters[i] == notIncludeClusterIndex) continue;

                ClusterIndexes.Add(clusters[i]);
            }
            EnterNodeIndex = startIndex;
            ExitNodeIndex = exitIndex;
        }

        public void Clear()
        {
            ClusterIndexes.Clear();
        }
    }

    public struct ClusterResult
    {
        public Vector2Int Index;
        public Vector2Int EnterDirection;
        public Vector2Int ExitDirection;
        public Vector2Int EntranceExit;
    }
}
