using UnityEngine;
using System.Collections.Generic;

public class HPACluster
{
    private readonly HPAGraph graph = new();
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

    public void Initialize(HPAClusterList clusterList, NodeList nodeList)
    {
        InitializeGraph(clusterList, nodeList);
    }

    private void InitializeGraph(HPAClusterList clusterList, NodeList nodeList)
    {
        var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // graph에 entrance node 추가
        for (int i = 0; i < directions.Length; i++)
        {
            var entrances = clusterList.SetEntrance(clusterIndex, directions[i]);
            if (entrances == null || entrances.Count == 0) continue;

            for (int j = 0; j < entrances.Count; j++)
            {
                if (graph.TryAddEntranceNode(entrances[j], directions[i], nodeList))
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

                float distance = pathfinder.FindPathInClusterForPathCache(entrance1, entrance2);
                if (distance > 0)
                {
                    graph.AddBidirectionalEdge(entrance1, entrance2, distance);
                }
            }
        }
    }

    private readonly List<Vector2Int> tempNodes = new();
    public void AddNodeToGraph(Vector2Int newNode, NodeList nodeList)
    {
        bool value = graph.TryAddNode(newNode, Vector2Int.zero, nodeList);
        if (value)
        {
            tempNodes.Add(newNode);
            foreach (var entrance in cachedEntrances)
            {
                float distance = pathfinder.FindPathInClusterForPathCache(entrance, newNode);
                if (distance > 0)
                {
                    graph.AddBidirectionalEdge(entrance, newNode, distance);
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

    public bool TryGetIntraEdgeCost(Vector2Int entrance1, Vector2Int entrance2, out float cost)
    {
        if (graph.TryGetEdgeWeight(entrance1, entrance2, out cost)) return true;

        if (graph.TryGetEdgeWeight(entrance2, entrance1, out cost)) return true;

        return false;
    }

    public void SetClusterActive(bool value) => IsActive = value;

    public bool IsNodeConnected(Vector2Int node1, Vector2Int node2) => graph.IsNodeConnected(node1, node2);
}
