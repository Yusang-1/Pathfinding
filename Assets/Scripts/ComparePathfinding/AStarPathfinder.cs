using UnityEngine;
using System.Collections.Generic;
using System;

public class AStarPathfinder : AbstractPathfinder
{
    private readonly NodeList nodeList;

    private readonly PriorityQueue<Vector2Int, float> openList = new();
    private readonly HashSet<Vector2Int> closeList = new();
    private readonly Dictionary<Vector2Int, PathNode> nodeDict = new();

    public IGetNeighborNodesActionProvider GetNeighborNodesActionProvider { get; private set; }

    public AStarPathfinder(NodeList nodeList)
    {
        this.nodeList = nodeList;
    }

    public override List<Vector3> FindPath(Vector3 start, Vector3 destination, float unitRadius)
    {
        Vector2Int startIndex = nodeList.GetNodeIndex(start);
        Vector2Int goalIndex = nodeList.GetNodeIndex(destination);

        List<Vector3> path = SearchAStar(startIndex, goalIndex, unitRadius, GetNeighborNodesActionProvider.GetNeighborNodes);
        return path;
    }

    public List<Vector3> FindPath(Vector2Int startNode, Vector2Int goalNode, float unitRadius)
    {
        List<Vector3> path = SearchAStar(startNode, goalNode, unitRadius, GetNeighborNodesActionProvider.GetNeighborNodes);
        return path;
    }

    public float FindPathLength(Vector2Int startNode, Vector2Int goalNode, float unitRadius)
    {
        List<Vector3> path = SearchAStar(startNode, goalNode, unitRadius, GetNeighborNodesActionProvider.GetNeighborNodes);

        float pathLength;
        if (path != null)
        {
            pathLength = PathResultRecorder.GetPathLength();
        }
        else
        {
            pathLength = 0;
        }
        
        Vector3ListPool.ReleaseValue(path);
        
        return pathLength;
    }

    public void SetGetNeighborPolicy(IGetNeighborNodesActionProvider policyProvider)
    {
        GetNeighborNodesActionProvider = policyProvider;
    }

    private List<Vector3> SearchAStar(Vector2Int startIndex, Vector2Int goalIndex, float unitRadius, Func<Vector2Int, float, List<Vector2Int>> getNeighborNodesAction)
    {
        openList.Clear();
        closeList.Clear();
        nodeDict.Clear();

        if (!nodeList.IsNodeAccessable(startIndex, goalIndex))
        {
            Debug.Log("접근 불가능한 노드입니다.");
            return null;
        }

        PathNode startNode = new PathNode
        {
            index = startIndex
        };
        openList.Enqueue(startNode.index, startNode.f);
        nodeDict[startNode.index] = startNode;

        while (openList.Count > 0)
        {
            Vector2Int current = openList.Dequeue();

            if (current == goalIndex)
            {
                PathResultRecorder.AddPathLength(nodeDict[current].g);
                PathResultRecorder.AddMemoryUsed(openList.Capacity + closeList.Count + nodeDict.Count);

                return CaculateResult(nodeDict, current, startIndex);
            }

            closeList.Add(current);

            List<Vector2Int> neighborList = getNeighborNodesAction(current, unitRadius);
            for (int i = 0; i < neighborList.Count; i++)
            {
                Vector2Int neighbor = neighborList[i];

                if (closeList.Contains(neighbor)) continue;

                PathResultRecorder.AddSearchedCount();

                float moveCost = GetMoveCost(current, neighbor);
                float newG = nodeDict[current].g + moveCost;

                if (!nodeDict.ContainsKey(neighbor) || newG < nodeDict[neighbor].g)
                {
                    if (!nodeDict.ContainsKey(neighbor))
                    {
                        nodeDict[neighbor] = new PathNode
                        {
                            index = neighbor,
                            h = CaculateHeuristic(neighbor, goalIndex)
                        };
                    }
                    PathNode neighborNode = nodeDict[neighbor];
                    neighborNode.g = newG;
                    neighborNode.parentIndex = current;
                    neighborNode.isParentSet = true;
                    nodeDict[neighbor] = neighborNode;

                    openList.Enqueue(nodeDict[neighbor].index, nodeDict[neighbor].f);
                }
            }
            Vector2IntListPool.ReleaseValue(neighborList);
        }

        // 경로 찾지 못함
        return null;
    }

    protected override float CaculateHeuristic(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);

        const float ORTHOGONAL_COST = 1f;
        const float DIAGONAL_COST = 1.4142f;
        // 대각선으로 이동 가능한 최대 거리 + 남은 수평/수직 거리        
        return (Mathf.Min(dx, dy) * DIAGONAL_COST) + (Mathf.Abs(dx - dy) * ORTHOGONAL_COST);
    }

    protected override List<Vector3> CaculateResult(Dictionary<Vector2Int, PathNode> nodes, Vector2Int current, Vector2Int start)
    {
        var path = Vector2IntListPool.GetValue();

        while (current != start)
        {
            path.Add(current);
            if (!nodes[current].isParentSet)
                break;
            current = nodes[current].parentIndex;
        }
        path.Add(start);
        path.Reverse();

        // 그리드 좌표를 월드 좌표로 변환
        var worldPath = Vector3ListPool.GetValue();
        foreach (var gridPos in path)
        {
            worldPath.Add(nodeList.GridToWorld(gridPos));

            nodeList.NodeTypeController.SetNodeTypeInPathFinding(gridPos, NodeType.trace);
        }
        Vector2IntListPool.ReleaseValue(path);

        return worldPath;
    }

    private float GetMoveCost(Vector2Int from, Vector2Int to) => from.GetNeighborMoveCost(to);
}
