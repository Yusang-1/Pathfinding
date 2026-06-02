using UnityEngine;
using System.Collections.Generic;

public class HPAClusterList
{
    private readonly HPAClusterOptimized[,] clusterList;
    private readonly NodeList nodeList;

    private readonly int clusterSize;
    private readonly int clusterCount;
    public int ClusterSize => clusterSize;

    public HPAClusterList(int nodeWidth, int clusterSize, NodeList nodeList)
    {
        this.clusterSize = clusterSize;
        this.nodeList = nodeList;

        clusterCount = nodeWidth / clusterSize;
        clusterList = new HPAClusterOptimized[clusterCount, clusterCount];

        cachedNeighborList = new List<Vector2Int>(4); // 상하좌우 최대 4개의 이웃
        cachedEdgeIndexes = new List<Vector2Int>(clusterSize);
        tempEdgeIndexes = new List<Vector2Int>(clusterSize);
    }

    public void Initialize(AStarPathfinder pathfinder)
    {
        // cluster 생성
        for (int i = 0; i < clusterList.GetLength(0); i++)
        {
            for (int j = 0; j < clusterList.GetLength(1); j++)
            {
                clusterList[i, j] = new HPAClusterOptimized(new Vector2Int(i, j), pathfinder);
            }
        }

        // cluster 초기화
        for (int i = 0; i < clusterList.GetLength(0); i++)
        {
            for (int j = 0; j < clusterList.GetLength(1); j++)
            {
                clusterList[i, j].Initialize(this, nodeList);
            }
        }
    }

    private readonly List<Vector2Int> cachedNeighborList;
    public List<Vector2Int> GetNeighborClusters(Vector2Int index)
    {
        cachedNeighborList.Clear();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i == 0 && j == 0) || i * j != 0) continue;

                int dx = index.x + i;
                int dy = index.y + j;
                if (dx < 0 || dy < 0 || dx >= clusterList.GetLength(0) || dy >= clusterList.GetLength(1)) continue;

                cachedNeighborList.Add(new Vector2Int(dx, dy));
            }
        }

        return cachedNeighborList;
    }

    private readonly List<Vector2Int> cachedEdgeIndexes;
    private readonly List<Vector2Int> tempEdgeIndexes;
    private const int entranceConstraint = 4; // 4보다 작은 너비의 입구는 중앙에 하나의 입구를 가짐, 4이상의 입구는 시작점과 끝점에 하나씩 가짐
    public List<Vector2Int> SetEntrance(Vector2Int cluster, Vector2Int direction)
    {
        if (cluster.x + direction.x < 0 || cluster.x + direction.x >= clusterCount
            || cluster.y + direction.y < 0 || cluster.y + direction.y >= clusterCount)
            return null;

        cachedEdgeIndexes.Clear();
        tempEdgeIndexes.Clear();
        Vector2Int standardNode = GetLeftDownNodeIndexOfCluster(cluster);

        if (direction.x == 0) // x축 경계선
        {
            if (direction.y > 0) // up
            {
                standardNode.y += clusterSize - 1;
            }

            for (int i = 0; i < clusterSize; i++) // x축을 따라 node 수집
            {
                if (nodeList.Nodes[standardNode.x, standardNode.y].IsWalkable && nodeList.Nodes[standardNode.x, standardNode.y + direction.y].IsWalkable)
                {
                    tempEdgeIndexes.Add(standardNode);
                }
                else // 막힌 edge발견시 tempEdgeIndexes에 있던 entrance들의 수를 파악해 1개 혹은 2개의 선별된 entrance를 cachedEdgeIndexes에 담는다
                {
                    if (tempEdgeIndexes.Count >= entranceConstraint)
                    {
                        cachedEdgeIndexes.Add(tempEdgeIndexes[0]);
                        cachedEdgeIndexes.Add(tempEdgeIndexes[^1]);
                    }
                    else
                    {
                        if (tempEdgeIndexes.Count == 0)
                        {
                            standardNode.x++;
                            continue;
                        }
                        
                        int mid = tempEdgeIndexes.Count / 2;
                        cachedEdgeIndexes.Add(tempEdgeIndexes[mid]);
                    }
                    tempEdgeIndexes.Clear();
                }

                standardNode.x++;
            }
        }
        else // y축 경계선
        {
            if (direction.x > 0) // right
            {
                standardNode.x += clusterSize - 1;
            }

            for (int i = 0; i < clusterSize; i++) // y축을 따라 node 수집
            {
                if (nodeList.Nodes[standardNode.x, standardNode.y].IsWalkable && nodeList.Nodes[standardNode.x + direction.x, standardNode.y].IsWalkable)
                {
                    tempEdgeIndexes.Add(standardNode);
                }
                else
                {
                    if (tempEdgeIndexes.Count >= entranceConstraint)
                    {
                        cachedEdgeIndexes.Add(tempEdgeIndexes[0]);
                        cachedEdgeIndexes.Add(tempEdgeIndexes[^1]);
                    }
                    else
                    {
                        if (tempEdgeIndexes.Count == 0)
                        {
                            standardNode.y++;
                            continue;
                        }

                        int mid = tempEdgeIndexes.Count / 2;
                        cachedEdgeIndexes.Add(tempEdgeIndexes[mid]);
                    }
                    tempEdgeIndexes.Clear();
                }

                standardNode.y++;
            }
        }

        if (tempEdgeIndexes.Count >= entranceConstraint)
        {
            cachedEdgeIndexes.Add(tempEdgeIndexes[0]);
            cachedEdgeIndexes.Add(tempEdgeIndexes[^1]);
        }
        else if (tempEdgeIndexes.Count > 0)
        {
            int mid = tempEdgeIndexes.Count / 2;
            cachedEdgeIndexes.Add(tempEdgeIndexes[mid]);
        }
        else if (cachedEdgeIndexes.Count == 0)
        {
            return null;
        }

        return cachedEdgeIndexes;
    }
    public IEnumerable<Vector2Int> GetEntrances(Vector2Int clusterIndex, Vector2Int direction)
    {
        return GetCluster(clusterIndex).Grpah.GetNodesByDirection(direction);
    }
    public List<Vector2Int> GetEntrancesOnce(Vector2Int clusterIndex, Vector2Int direction)
    {
        return GetCluster(clusterIndex).Grpah.GetNodesByDirectionOnce(direction);
    }

    private Vector2Int GetLeftDownNodeIndexOfCluster(Vector2Int clusterIndex) => clusterIndex * clusterSize;
    public HPAClusterOptimized GetCluster(Vector2Int index) => clusterList[index.x, index.y];
    public Vector2Int GetClusterIndex(int x, int y) => new(x / clusterSize, y / clusterSize);
    public Vector2Int GetClusterIndex(Vector2Int index) => new(index.x / clusterSize, index.y / clusterSize);

    public bool IsNodeInCluster(Vector2Int clusterIndex, Vector2Int nodeIndex)
    {
        var nodeStandard = GetLeftDownNodeIndexOfCluster(clusterIndex);
        int xMin = nodeStandard.x;
        int yMin = nodeStandard.y;

        if (nodeIndex.x >= xMin && nodeIndex.x < xMin + clusterSize
            && nodeIndex.y >= yMin && nodeIndex.y < yMin + clusterSize)
        {
            return true;
        }
        else return false;
    }

    public bool IsNodesInSameCluster(Vector2Int node1, Vector2Int node2)
    {
        if (GetClusterIndex(node1) == GetClusterIndex(node2)) return true;
        else return false;
    }

    public void ResetClusterList()
    {
        for (int i = 0; i < clusterList.GetLength(0); i++)
        {
            for (int j = 0; j < clusterList.GetLength(1); j++)
            {
                SetClusterActive(new Vector2Int(i, j), false);                
            }
        }
    }

    public void SetAllCLusterActive()
    {
        for (int i = 0; i < clusterList.GetLength(0); i++)
        {
            for (int j = 0; j < clusterList.GetLength(1); j++)
            {
                SetClusterActive(new Vector2Int(i, j), true);
            }
        }
    }

    public void SetClusterActive(Vector2Int index, bool value) => clusterList[index.x, index.y].SetClusterActive(value);
}
