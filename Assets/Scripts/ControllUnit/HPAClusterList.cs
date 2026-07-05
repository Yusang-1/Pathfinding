using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
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

        public void Initialize(AStarPathfinder pathfinder, int mapSize, int clusterSize, Dictionary<UnitSize, float> unitRadiusList)
        {
            this.clusterSize = clusterSize;
            clusterCount = mapSize / clusterSize;
            clusterList = new HPACluster[clusterCount, clusterCount];

            cachedEdgeIndexes = new List<HPAGraph.EntranceData>(clusterSize);
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
                    clusterList[i, j].Initialize(this, nodeList, unitRadiusList);
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
                    if (i * j != 0) continue; // 대각선 제외
                    if (i == 0 && j == 0) continue; // 자신 제외

                    int dx = index.x + i;
                    int dy = index.y + j;
                    if (dx < 0 || dy < 0 || dx >= clusterList.GetLength(0) || dy >= clusterList.GetLength(1)) continue;

                    cachedNeighborList.Add(new Vector2Int(dx, dy));
                }
            }

            return cachedNeighborList;
        }

        private List<HPAGraph.EntranceData> cachedEdgeIndexes;
        private List<Vector2Int> tempEdgeIndexes;
        public List<HPAGraph.EntranceData> SetEntrance(Vector2Int cluster, Vector2Int direction)
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

            // entrance가 중간에 가로막히지 않았을 경우
            if (tempEdgeIndexes.Count > 0)
            {
                GetCachedIndexes(tempEdgeIndexes, cachedEdgeIndexes, out isSuccess);
                if (!isSuccess) return null;
            }

            return cachedEdgeIndexes;
        }
        private void GetCachedIndexes(List<Vector2Int> tempEdges, List<HPAGraph.EntranceData> cachedEdges, out bool isSuccess)
        {
            if (tempEdges.Count > 0)
            {
                HPAGraph.EntranceData entranceData = new()
                {
                    LeftEntrance = tempEdges[0],
                    RightEntrance = tempEdges[^1]
                };
                cachedEdges.Add(entranceData);

                isSuccess = true;
            }
            else isSuccess = false;
        }

        public IEnumerable<Vector2Int> GetEntrances(Vector2Int clusterIndex, Vector2Int direction, float unitRadius)
        {
            return GetCluster(clusterIndex).Graph.GetNodesByDirection(direction, unitRadius);
        }
        public List<Vector2Int> GetEntrancesOnce(Vector2Int clusterIndex, Vector2Int direction, float unitRadius)
        {
            return GetCluster(clusterIndex).Graph.GetNodesByDirectionOnce(direction, unitRadius);
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
}
