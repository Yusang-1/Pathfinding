using UnityEngine;
using System.Collections.Generic;
using System;

public class AStarPathfinder : AbstractPathfinder
{
    private readonly NodeList nodeList;
    private readonly HPAClusterList hPAClusterList;

    private readonly PriorityQueue<Vector2Int, float> openList = new();
    private readonly HashSet<Vector2Int> closeList = new();
    private readonly Dictionary<Vector2Int, PathNode> nodeDict = new();

    public AStarPathfinder(NodeList nodeList, HPAClusterList hPAClusterList)
    {
        this.nodeList = nodeList;
        this.hPAClusterList = hPAClusterList;
        directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    }

    public override List<Vector3> FindPath(Vector3 from, Vector3 to)
    {
        List<Vector3> path = SearchAStar(from, to, GetNeighborNode);
        return path;
    }

    /// <summary> 같은 cluster List내에서 경로를 찾음 </summary>
    public List<Vector3> FindPathInClusterList(Vector3 from, Vector3 to, List<Vector2Int> clusters)
    {
        clusterListToFind = clusters;
        List<Vector3> path = SearchAStar(from, to, GetNeightborNodesInClusterList);
        return path;
    }

    /// <summary> 같은 cluster내에서 경로를 찾아 길이를 반환 </summary>
    public float FindPathInClusterForPathCache(Vector2Int from, Vector2Int to)
    {
        Vector3 fromPos = new(from.x, from.y);
        Vector3 toPos = new(to.x, to.y);

        List<Vector3> path = SearchAStar(fromPos, toPos, GetNeighborNodesInCluster);

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

    private List<Vector3> SearchAStar(Vector3 startPosition, Vector3 destinationPosition, Func<Vector2Int, List<Vector2Int>> getNeighbors)
    {
        openList.Clear();
        closeList.Clear();
        nodeDict.Clear();

        Vector2Int startIndex = nodeList.GetNodeIndex(startPosition);
        Vector2Int goalIndex = nodeList.GetNodeIndex(destinationPosition);

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

            List<Vector2Int> neighborList = getNeighbors(current);
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

    private readonly Vector2Int[] directions;
    protected override List<Vector2Int> GetNeighborNode(Vector2Int current)
    {
        List<Vector2Int> neighbors = Vector2IntListPool.GetValue();

        for (int i = 0; i < directions.Length; i++)
        {
            int newX = current.x + directions[i].x;
            int newY = current.y + directions[i].y;

            if (newX < 0 || newY < 0 ||
                newX >= nodeList.Nodes.GetLength(0) || newY >= nodeList.Nodes.GetLength(0))
            {
                continue;
            }

            Vector2Int neighbor = new(newX, newY);
            // 워크어빌리티 맵으로 확인
            var s = nodeList.GridToWorld(neighbor);
            var c = hPAClusterList.GetClusterIndex((int)s.x, (int)s.y);
            if (nodeList.Nodes[newX, newY].IsWalkable && hPAClusterList.GetCluster(c).IsActive)
            {
                nodeList.SetNodeTypeInPathFinding(neighbor, NodeType.searched);
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private List<Vector2Int> GetNeighborNodesInCluster(Vector2Int current)
    {
        List<Vector2Int> neighbors = Vector2IntListPool.GetValue();

        for (int i = 0; i < directions.Length; i++)
        {
            int newX = current.x + directions[i].x;
            int newY = current.y + directions[i].y;

            Vector2Int neighbor = new(newX, newY);

            if (newX < 0 || newY < 0
                || newX >= nodeList.Nodes.GetLength(0) || newY >= nodeList.Nodes.GetLength(0)
                || !hPAClusterList.IsNodeInCluster(hPAClusterList.GetClusterIndex(current), neighbor))
            {
                continue;
            }

            // 워크어빌리티 맵으로 확인      
            if (nodeList.Nodes[newX, newY].IsWalkable)
            {
                nodeList.SetNodeTypeInPathFinding(neighbor, NodeType.searched);
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private List<Vector2Int> clusterListToFind;
    private List<Vector2Int> GetNeightborNodesInClusterList(Vector2Int current)
    {
        List<Vector2Int> neighbors = Vector2IntListPool.GetValue();

        for (int i = 0; i < directions.Length; i++)
        {
            int newX = current.x + directions[i].x;
            int newY = current.y + directions[i].y;

            Vector2Int neighbor = new(newX, newY);

            if (newX < 0 || newY < 0 || newX >= nodeList.Nodes.GetLength(0) || newY >= nodeList.Nodes.GetLength(0))
            {
                continue;
            }

            bool isNeighborInClusters = false;
            foreach (var cluster in clusterListToFind)
            {
                isNeighborInClusters = isNeighborInClusters || hPAClusterList.IsNodeInCluster(cluster, neighbor);

                if (isNeighborInClusters) break;
            }
            if (!isNeighborInClusters) continue;

            // 워크어빌리티 맵으로 확인      
            if (nodeList.Nodes[newX, newY].IsWalkable)
            {
                nodeList.SetNodeTypeInPathFinding(neighbor, NodeType.searched);
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
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

            nodeList.SetNodeTypeInPathFinding(gridPos, NodeType.trace);
        }
        Vector2IntListPool.ReleaseValue(path);

        return worldPath;
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

    private float GetMoveCost(Vector2Int from, Vector2Int to) => from.GetNeighborMoveCost(to);
}
