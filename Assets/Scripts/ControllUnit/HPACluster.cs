using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class HPACluster
    {
        private HPAGraph graph;
        private readonly Vector2Int clusterIndex;
        private readonly AStarPathfinder pathfinder;

        private readonly HashSet<Vector2Int> cachedEntrances = new();

        public HPAGraph Graph => graph;
        public bool IsActive { get; private set; }

        public HPACluster(Vector2Int index, AStarPathfinder pathfinder)
        {
            clusterIndex = index;
            this.pathfinder = pathfinder;
        }

        public void Initialize(HPAClusterList clusterList, Dictionary<UnitSize, float> unitRadiusDict)
        {
            graph = new HPAGraph(unitRadiusDict);

            foreach (var radius in unitRadiusDict.Values)
            {
                InitializeGraph(clusterList, radius);
            }
        }

        private void InitializeGraph(HPAClusterList clusterList, float unitRadius)
        {
            var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            // graph에 entrance node 추가
            for (int i = 0; i < directions.Length; i++)
            {
                var entrances = clusterList.SetEntrance(clusterIndex, directions[i], unitRadius);
                if (entrances == null || entrances.Count == 0) continue;

                for (int j = 0; j < entrances.Count; j++)
                {
                    if (graph.TryAddEntranceNode(entrances[j], directions[i], unitRadius))
                    {
                        if (entrances[j].LeftEntrance != entrances[j].RightEntrance)
                        {
                            cachedEntrances.Add(entrances[j].LeftEntrance);
                            cachedEntrances.Add(entrances[j].RightEntrance);
                        }
                        else
                            cachedEntrances.Add(entrances[j].LeftEntrance);
                    }
                    else Debug.LogWarning("그래프에 노드 추가 실패");
                }
            }

            // intra-cluster 간선 계산
            var entranceList = new List<Vector2Int>(cachedEntrances);
            for (int i = 0; i < entranceList.Count; i++)
            {
                for (int j = i + 1; j < entranceList.Count; j++)
                {
                    var entrance1 = entranceList[i];
                    var entrance2 = entranceList[j];

                    float distance = pathfinder.FindPathInClusterForPathCache(entrance1, entrance2, unitRadius);
                    if (distance > 0)
                    {
                        graph.AddBidirectionalEdge(entrance1, entrance2, distance, unitRadius);
                    }
                }
            }
        }

        private readonly List<Vector2Int> tempNodes = new();
        public void AddNodeToGraph(Vector2Int newNode, float unitRadius)
        {
            bool isAddNodeSuccess = graph.TryAddNode(newNode, Vector2Int.zero, unitRadius);
            if (isAddNodeSuccess)
            {
                tempNodes.Add(newNode);
                // cluster에 존재하는 entrance들과 추가한 node의 경로 캐싱
                foreach (var entrance in cachedEntrances)
                {
                    float distance = pathfinder.FindPathInClusterForPathCache(entrance, newNode, unitRadius);
                    if (distance > 0)
                    {
                        graph.AddBidirectionalEdge(entrance, newNode, distance, unitRadius);
                    }
                }
                cachedEntrances.Add(newNode);
            }
        }
        
        public void RemoveTempNodeInGraph()
        {
            foreach (var node in tempNodes)
            {
                graph.RemoveTempNode(node);
                cachedEntrances.Remove(node);
            }
        }

        public bool TryGetIntraEdgeCost(Vector2Int entrance1, Vector2Int entrance2, out float cost, float unitRadius)
        {
            if (graph.TryGetEdgeWeight(entrance1, entrance2, out cost, unitRadius)) return true;

            if (graph.TryGetEdgeWeight(entrance2, entrance1, out cost, unitRadius)) return true;

            return false;
        }

        public void SetClusterActive(bool value) => IsActive = value;

        public bool IsNodeConnected(Vector2Int node1, Vector2Int node2, float unitRadius) => graph.IsNodeConnected(node1, node2, unitRadius);
    }
}
