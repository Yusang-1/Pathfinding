using UnityEngine;
using System.Collections.Generic;

public class AStarPathfinder : AbstractPathfinder
{
    private readonly NodeList nodeList;
    private readonly HPAClusterList hPAClusterList;
    
    private int numberOfNodesSearched;
    
    public AStarPathfinder(NodeList nodeList, HPAClusterList hPAClusterList)
    {
        this.nodeList = nodeList;
        this.hPAClusterList = hPAClusterList;
        directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    }

    public override List<Vector3> FindPath(Vector3 startPosition, Vector3 destinationPosition)
    {
        PriorityQueue<Vector2Int, float> openList = new();
        HashSet<Vector2Int> closeList = new();
        Dictionary<Vector2Int, PathNode> nodeDict = new();

        Vector2Int startIndex = nodeList.GetNodeIndex(startPosition);
        Vector2Int goalIndex = nodeList.GetNodeIndex(destinationPosition);
        numberOfNodesSearched = 0;
        
        if(!nodeList.IsNodeAccessable(startIndex, goalIndex))
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
                Debug.Log($"A*의 탐색한 노드 수 : {numberOfNodesSearched}");
                return CaculateResult(nodeDict, current, startIndex);
            }

            closeList.Add(current);

            List<Vector2Int> neighborList = GetNeighborNode(current);
            for (int i = 0; i < neighborList.Count; i++)
            {
                Vector2Int neighbor = neighborList[i];

                if (closeList.Contains(neighbor)) continue;
                
                numberOfNodesSearched++;
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
        }

        // 경로 찾지 못함
        return new List<Vector3>();
    }
    public float FindPath(Vector2Int startIndex, Vector2Int goalIndex)
    {
        PriorityQueue<Vector2Int, float> openList = new();
        HashSet<Vector2Int> closeList = new();
        Dictionary<Vector2Int, PathNode> nodeDict = new();

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
                return nodeDict[current].g;
            }

            closeList.Add(current);

            List<Vector2Int> neighborList = GetNeighborNode(current);
            for (int i = 0; i < neighborList.Count; i++)
            {
                Vector2Int neighbor = neighborList[i];

                if (closeList.Contains(neighbor)) continue;

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
        }

        // 경로 찾지 못함
        return 0;
    }
    public float FindPathInClusterForPathCache(Vector2Int from, Vector2Int to, HPAClusterList clusterList)
    {
        PriorityQueue<Vector2Int, float> openList = new();
        HashSet<Vector2Int> closeList = new();
        Dictionary<Vector2Int, PathNode> nodeDict = new();

        Vector2Int startIndex = nodeList.GetNodeIndex(from);
        Vector2Int goalIndex = nodeList.GetNodeIndex(to);

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
                return nodeDict[current].g;
            }
            closeList.Add(current);
                        
            var neighbors = GetNeighborNodesInCluster(current, clusterList);
            for(int i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];
                
                if (closeList.Contains(neighbor)) continue;
                
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
        }
        
        // 경로 찾지 못함
        return 0;
    }

    private readonly Vector2Int[] directions;
    protected override List<Vector2Int> GetNeighborNode(Vector2Int current)
    {
        List<Vector2Int> neighbors = new();

        // 상하좌우 + 대각선 (8방향)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;  // 자신은 제외
                if (dx * dy != 0) continue; // 대각선 제외

                int newX = current.x + dx;
                int newY = current.y + dy;

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
        }

        return neighbors;
    }
    private List<Vector2Int> GetNeighborNodesInCluster(Vector2Int current, HPAClusterList clusterList)
    {
        List<Vector2Int> neighbors = new();

        // 상하좌우 + 대각선 (8방향)
        for (int i = 0; i < directions.Length; i++)
        {
            int newX = current.x + directions[i].x;
            int newY = current.y + directions[i].y;
            
            Vector2Int neighbor = new(newX, newY);
            
            if (newX < 0 || newY < 0
                || newX >= nodeList.Nodes.GetLength(0) || newY >= nodeList.Nodes.GetLength(0)
                || !hPAClusterList.IsNodeInCluster(clusterList.GetClusterIndex(current), neighbor))
            {
                continue;
            }
            
            // 워크어빌리티 맵으로 확인      
            if (nodeList.Nodes[newX, newY].IsWalkable)
            {                
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    protected override List<Vector3> CaculateResult(Dictionary<Vector2Int, PathNode> nodes, Vector2Int current, Vector2Int start)
    {
        var path = new List<Vector2Int>();

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
        var worldPath = new List<Vector3>();
        foreach (var gridPos in path)
        {
            worldPath.Add(nodeList.GridToWorld(gridPos));

            nodeList.SetNodeTypeInPathFinding(gridPos, NodeType.trace);
        }

        return worldPath;
    }

    protected override float CaculateHeuristic(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);

        // 대각선으로 이동 가능한 최대 거리 + 남은 수평/수직 거리
        // return (Mathf.Min(dx, dy) * DIAGONAL_COST) + (Mathf.Abs(dx - dy) * ORTHOGONAL_COST);
        return dx + dy;
    }

    private const float ORTHOGONAL_COST = 1f;  // 상하좌우 비용
    private const float DIAGONAL_COST = 1.414213562f;  // 대각선 비용 sqrt(2)
    private float GetMoveCost(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);

        // 대각선 이동
        if (dx != 0 && dy != 0)
            return DIAGONAL_COST;
        // 상하좌우 이동
        else
            return ORTHOGONAL_COST;
    }
}
