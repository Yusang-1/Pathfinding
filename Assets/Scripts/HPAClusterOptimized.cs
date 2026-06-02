using UnityEngine;
using System.Collections.Generic;

public class HPAClusterOptimized
{
    private readonly HPAGraph graph = new();
    private readonly Vector2Int clusterIndex;
    private readonly AStarPathfinder pathfinder;

    private readonly HashSet<Vector2Int> cachedEntrances = new();
    private HPAClusterList clusterList;

    public HPAGraph Grpah => graph;
    public bool IsActive { get; private set; }

    public HPAClusterOptimized(Vector2Int index, AStarPathfinder pathfinder)
    {
        clusterIndex = index;
        this.pathfinder = pathfinder;
    }

    public void Initialize(HPAClusterList clusterList, NodeList nodeList)
    {
        this.clusterList = clusterList;
        InitializeGraph(clusterList, nodeList);
    }

    private void InitializeGraph(HPAClusterList clusterList, NodeList nodeList)
    {
        var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // graph에 entrance node 추가
        for (int i = 0; i < directions.Length; i++)
        {
            var entrances = clusterList.SetEntrance(clusterIndex, directions[i]);
            if (entrances != null)
            {
                for (int j = 0; j < entrances.Count; j++)
                {
                    graph.AddNode(entrances[j], directions[i], nodeList);
                    cachedEntrances.Add(entrances[j]);
                }
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

                float distance = pathfinder.FindPathInClusterForPathCache(entrance1, entrance2, clusterList);
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
        tempNodes.Add(newNode);
        graph.AddNode(newNode, Vector2Int.zero, nodeList);
        foreach (var entrance in cachedEntrances)
        {
            float distance = pathfinder.FindPathInClusterForPathCache(entrance, newNode, clusterList);
            if (distance > 0)
            {
                graph.AddBidirectionalEdge(entrance, newNode, distance);
            }
        }
    }
    public void RemoveTempNodeInGraph()
    {
        foreach (var node in tempNodes)
        {
            graph.RemoveTempNode(node);
        }
    }

    public float GetHeuristic(Vector2Int from, Vector2Int to)
    {
        return Vector2Int.Distance(from, to);
    }

    public IEnumerable<Vector2Int> GetPath(Vector2Int start, Vector2Int goal)
    {
        return AStarHPA(start, goal);
    }

    private List<Vector2Int> AStarHPA(Vector2Int start, Vector2Int goal)
    {
        PriorityQueue<Vector2Int, float> openSet = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> gScore = new() { { start, 0 } };
        Dictionary<Vector2Int, float> fScore = new() { { start, GetHeuristic(start, goal) } };

        openSet.Enqueue(start, fScore[start]);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!graph.TryGetEdgeWeight(current, neighbor, out float edgeWeight)) continue;

                float tentativeGScore = gScore[current] + edgeWeight;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + GetHeuristic(neighbor, goal);
                    openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return null;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
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
