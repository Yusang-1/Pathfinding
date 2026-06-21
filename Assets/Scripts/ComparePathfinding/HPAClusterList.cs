using UnityEngine;
using System.Collections.Generic;

public class HPAClusterList
{
    private HPACluster[,] clusterList;
    private readonly NodeList nodeList;

    private int clusterSize;
    private int clusterCount;

    public HPAClusterList(NodeList nodeList)
    {
        this.nodeList = nodeList;

        cachedNeighborList = new List<Vector2Int>(4); // 상하좌우 최대 4개의 이웃
    }

    public void Initialize(AStarPathfinder pathfinder, int mapSize, int clusterSize)
    {
        this.clusterSize = clusterSize;
        clusterCount = mapSize / clusterSize;
        clusterList = new HPACluster[clusterCount, clusterCount];

        cachedEdgeIndexes = new List<Vector2Int>(clusterSize);
        tempEdgeIndexes = new List<Vector2Int>(clusterSize);

        // cluster 생성
        for (int i = 0; i < clusterList.GetLength(0); i++)
        {
            for (int j = 0; j < clusterList.GetLength(1); j++)
            {
                clusterList[i, j] = new HPACluster(new Vector2Int(i, j), pathfinder);
            }
        }

        // cluster 초기화
        for (int i = 0; i < clusterList.GetLength(0); i++)
        {
            for (int j = 0; j < clusterList.GetLength(1); j++)
            {
                clusterList[i, j].Initialize(this, nodeList);
                nodeList.NodeInfo.ResetSearched();
                nodeList.NodeInfo.ResetTrace();
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
                if (i == 0 && j == 0) continue;

                int dx = index.x + i;
                int dy = index.y + j;
                if (dx < 0 || dy < 0 || dx >= clusterList.GetLength(0) || dy >= clusterList.GetLength(1)) continue;

                cachedNeighborList.Add(new Vector2Int(dx, dy));
            }
        }

        return cachedNeighborList;
    }

    private List<Vector2Int> cachedEdgeIndexes;
    private List<Vector2Int> tempEdgeIndexes;
    public List<Vector2Int> SetEntrance(Vector2Int cluster, Vector2Int direction)
    {
        if (cluster.x + direction.x < 0 || cluster.x + direction.x >= clusterCount
            || cluster.y + direction.y < 0 || cluster.y + direction.y >= clusterCount)
            return null;

        cachedEdgeIndexes.Clear();
        tempEdgeIndexes.Clear();
        Vector2Int standardNode = GetLeftDownNodeIndexOfCluster(cluster);
        bool isSuccess;
        if (direction.x == 0) // x축 경계선
        {
            if (direction.y > 0) // up
            {
                standardNode.y += clusterSize - 1;
            }

            for (int i = 0; i < clusterSize; i++) // x축을 따라 node 수집
            {
                if (nodeList.GetNode(standardNode).IsWalkable && nodeList.GetNode(standardNode + direction).IsWalkable)
                {
                    tempEdgeIndexes.Add(standardNode);
                }
                else // 막힌 edge발견시 tempEdgeIndexes에 있던 entrance들의 수를 파악해 선별된 entrance를 cachedEdgeIndexes에 담는다
                {
                    GetCachedIndexes(tempEdgeIndexes, cachedEdgeIndexes, out isSuccess);

                    if (!isSuccess)
                    {
                        standardNode.x++;
                        continue;
                    }
                    tempEdgeIndexes.Clear();
                }

                standardNode.x++;
            }
        }
        else if (direction.y == 0) // y축 경계선
        {
            if (direction.x > 0) // right
            {
                standardNode.x += clusterSize - 1;
            }

            for (int i = 0; i < clusterSize; i++) // y축을 따라 node 수집
            {
                if (nodeList.GetNode(standardNode).IsWalkable && nodeList.GetNode(standardNode + direction).IsWalkable)
                {
                    tempEdgeIndexes.Add(standardNode);
                }
                else
                {
                    GetCachedIndexes(tempEdgeIndexes, cachedEdgeIndexes, out isSuccess);

                    if (!isSuccess)
                    {
                        standardNode.x++;
                        continue;
                    }
                    tempEdgeIndexes.Clear();
                }

                standardNode.y++;
            }
        }
        else // 대각선
        {
            if(direction.x > 0) standardNode.x += clusterSize - 1;
            if(direction.y > 0) standardNode.y += clusterSize - 1;
            
            if (nodeList.GetNode(standardNode).IsWalkable && nodeList.GetNode(standardNode + direction).IsWalkable)
            {
                cachedEdgeIndexes.Add(standardNode);
            }
        }

        // entrance가 중간에 가로막히지 않았을 경우
        if (tempEdgeIndexes.Count > 0)
        {
            GetCachedIndexes(tempEdgeIndexes, cachedEdgeIndexes, out isSuccess);
            if (!isSuccess) return null;
        }

        return cachedEdgeIndexes;
    }
    private void GetCachedIndexes(List<Vector2Int> tempEdges, List<Vector2Int> cachedEdges, out bool isSuccess)
    {
        const int entranceConstraint = 3; // 3이하 너비의 입구는 중앙에 하나의 입구를 가짐, 3초과의 입구는 시작점과 끝점에 하나씩 가짐
        const int bigEntrance = 9; // 9이상 너비의 입구는 시작, 중간, 끝에 입구를 가짐

        isSuccess = true;
        if (tempEdges.Count >= bigEntrance)
        {
            int mid = tempEdges.Count / 2;
            cachedEdges.Add(tempEdges[0]);
            cachedEdges.Add(tempEdges[mid]);
            cachedEdges.Add(tempEdges[^1]);
        }
        else if (tempEdges.Count > entranceConstraint)
        {
            cachedEdges.Add(tempEdges[0]);
            cachedEdges.Add(tempEdges[^1]);
        }
        else if (tempEdges.Count > 0)
        {
            int mid = tempEdges.Count / 2;
            cachedEdges.Add(tempEdges[mid]);
        }
        else isSuccess = false;
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
    public HPACluster GetCluster(Vector2Int index) => clusterList[index.x, index.y];
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
