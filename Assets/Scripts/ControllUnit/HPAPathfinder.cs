using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class HPAPathfinder
    {
        private readonly HPAClusterList clusterList;
        private readonly NodeList nodeList;

        private readonly List<ResultNode> results = new();

        // 메모리 풀: List 재사용으로 GC 감소
        private readonly Stack<List<Vector2Int>> listPool = new();
        private const int PoolSize = 10;

        private readonly PriorityQueue<AbstractNode, float> openSet = new();
        private readonly HashSet<AbstractNode> closedSet = new();
        private readonly Dictionary<AbstractNode, (float h, float g)> clusterDict = new();
        // parent map: maps a node to its predecessor for path reconstruction
        private readonly Dictionary<AbstractNode, AbstractNode> cameFrom = new();

        public HPAPathfinder(HPAClusterList clusterList, NodeList nodeList)
        {
            this.clusterList = clusterList;
            this.nodeList = nodeList;

            // 메모리 풀 초기화
            for (int i = 0; i < PoolSize; i++)
            {
                listPool.Push(new List<Vector2Int>());
            }
        }


        /// <summary> high level cluster 경로를 반환 </summary>
        public List<ResultNode> FindClusterPath(Vector3 from, Vector3 to, out PathResult pathResult, float unitRadius)
        {
            results.Clear();
            Vector2Int startNode = nodeList.GetNodeIndex(from);
            Vector2Int goalNode = nodeList.GetNodeIndex(to);

            pathResult = new();
            if (!IsWalkable(startNode) || !IsWalkable(goalNode) || !nodeList.IsNodeAccessable(startNode, goalNode))
            {
                Debug.Log("접근 불가능한 노드입니다.");
                return null;
            }

            Vector2Int startCluster = clusterList.GetClusterIndex(startNode);
            Vector2Int goalCluster = clusterList.GetClusterIndex(goalNode);

            // start cluster, goal cluster에 노드 추가
            clusterList.GetCluster(startCluster).AddNodeToGraph(startNode, nodeList, unitRadius);
            clusterList.GetCluster(goalCluster).AddNodeToGraph(goalNode, nodeList, unitRadius);
            nodeList.NodeInfo.ResetSearched();
            nodeList.NodeInfo.ResetTrace();

            List<ResultNode> clusterPath;
            // from과 to가 같은 클러스터에 존재하고 startNode에서 goalNode로 이동 가능한 경우 resultNode하나 리턴
            if (startCluster == goalCluster && clusterList.GetCluster(startCluster).IsNodeConnected(startNode, goalNode, unitRadius))
            {
                return new() {
                new ResultNode{
                    Index = startCluster, enteranceNode = startNode, exitNode = goalNode, hasEntranceAndExit = true
                }
            };
            }
            else
            {
                // 고수준 클러스터 경로 탐색
                clusterPath = FindAbstractClusterPath(startCluster, goalCluster, startNode, goalNode, out PathResult result, unitRadius);
                pathResult.AddResult(result);
            }

            // 시작지점과 도착지점을 clusterPath에 추가
            var temp = clusterPath[0];
            temp.enteranceNode = startNode;
            temp.hasEntranceAndExit = true;
            clusterPath[0] = temp;

            temp = clusterPath[^1];
            temp.exitNode = goalNode;
            temp.hasEntranceAndExit = true;
            clusterPath[^1] = temp;

            // start cluster, goal cluster에 추가된 노드 제거
            clusterList.GetCluster(startCluster).RemoveTempNodeInGraph();
            clusterList.GetCluster(goalCluster).RemoveTempNodeInGraph();

            return clusterPath;
        }

        /// <summary> 고수준 클러스터 경로 탐색 </summary>    
        private List<ResultNode> FindAbstractClusterPath(Vector2Int startCluster, Vector2Int goalCluster, Vector2Int startNode, Vector2Int goalNode, out PathResult pathResult, float unitRadius)
        {
            openSet.Clear();
            closedSet.Clear();
            clusterDict.Clear();
            cameFrom.Clear();

            pathResult = new();
            List<Vector2Int> startEntrances = GetAllEntrances(startCluster, unitRadius);
            if (startEntrances == null || startEntrances.Count == 0) return null;

            AbstractNode startVirtual = new()
            {
                Index = startCluster,
                EntrancePos = startNode,
            };
            openSet.Enqueue(startVirtual, 0);
            clusterDict[startVirtual] = (0, 0);
            cameFrom[startVirtual] = startVirtual;

            while (openSet.Count > 0)
            {
                AbstractNode current = openSet.Dequeue();
                if (current.Index == goalCluster && clusterList.GetCluster(current.Index).IsNodeConnected(current.EntrancePos, goalNode, unitRadius))
                {
                    pathResult.MemoryUsed += openSet.Capacity;
                    pathResult.MemoryUsed += clusterDict.Count;
                    pathResult.MemoryUsed += closedSet.Count;
                    return ReconstructAbstractPath(clusterDict, cameFrom, current, startVirtual);
                }
                if (closedSet.Contains(current)) continue;
                closedSet.Add(current);

                foreach (var (neighbor, cost) in GetAbstractNeighbors(current, unitRadius))
                {
                    if (closedSet.Contains(neighbor)) continue;

                    pathResult.SearchedCount++;
                    float tentativeG = clusterDict[current].g + cost;

                    if (!clusterDict.ContainsKey(neighbor) || tentativeG < clusterDict[neighbor].g)
                    {
                        if (!clusterDict.ContainsKey(neighbor))
                        {
                            clusterDict[neighbor] = (CaculateHeuristic(neighbor.EntrancePos, goalNode), 0);
                        }
                        var item = clusterDict[neighbor];
                        item.g = tentativeG;
                        clusterDict[neighbor] = item;

                        // record parent for reconstruction
                        cameFrom[neighbor] = current;

                        openSet.Enqueue(neighbor, clusterDict[neighbor].g + clusterDict[neighbor].h);
                    }
                }
            }

            return null; // 경로 없음
        }

        private List<ResultNode> ReconstructAbstractPath(Dictionary<AbstractNode, (float h, float g)> clusterDict, Dictionary<AbstractNode, AbstractNode> cameFrom, AbstractNode current, AbstractNode start)
        {
            var nodes = new Dictionary<Vector2Int, int>();

            // 도착지 노드 세팅 (current는 목표 노드)
            results.Add(new ResultNode
            {
                Index = current.Index,
                enteranceNode = current.EntrancePos,
                hasEntranceAndExit = true
            });

            nodes.Add(current.Index, results.Count - 1);
            // move to parent using cameFrom
            current = cameFrom[current];

            ResultNode temp;
            while (!current.Equals(start))
            {
                if (nodes.ContainsKey(current.Index))
                {
                    if (!results[nodes[current.Index]].hasEntranceAndExit)
                    {
                        temp = results[nodes[current.Index]];
                        temp.enteranceNode = current.EntrancePos;
                        temp.hasEntranceAndExit = true;
                        results[nodes[current.Index]] = temp;
                    }
                    else
                    {
                        results.Add(new ResultNode
                        {
                            Index = current.Index,
                            exitNode = current.EntrancePos,
                        });
                        nodes[current.Index] = results.Count - 1;
                    }
                }
                else
                {
                    results.Add(new ResultNode
                    {
                        Index = current.Index,
                        exitNode = current.EntrancePos,
                    });

                    nodes.Add(current.Index, results.Count - 1);
                }

                current = cameFrom[current];
            }

            if (nodes.ContainsKey(current.Index)) // 출발지 노드를 별도로 세팅
            {
                if (!results[nodes[current.Index]].hasEntranceAndExit)
                {
                    temp = results[nodes[current.Index]];
                    temp.enteranceNode = current.EntrancePos;
                    temp.hasEntranceAndExit = true;
                    results[nodes[current.Index]] = temp;
                }
                else
                {
                    results.Add(new ResultNode
                    {
                        Index = current.Index,
                        exitNode = current.EntrancePos,
                    });
                    nodes[current.Index] = results.Count - 1;
                }
            }
            else
            {
                results.Add(new ResultNode
                {
                    Index = current.Index,
                    exitNode = current.EntrancePos,
                    hasEntranceAndExit = true
                });
            }

            results.Reverse();
            return results;
        }

        private IEnumerable<(AbstractNode node, float cost)> GetAbstractNeighbors(AbstractNode current, float unitRadius)
        {
            var cluster = clusterList.GetCluster(current.Index);

            // Intra-cluster edges
            List<Vector2Int> entranceList = GetAllEntrances(current.Index, unitRadius);
            foreach (var other in entranceList)
            {
                if (other == current.EntrancePos) continue;

                if (cluster.TryGetIntraEdgeCost(current.EntrancePos, other, out float intraCost, unitRadius))
                {
                    yield return (
                        new AbstractNode { Index = current.Index, EntrancePos = other },
                        intraCost
                    );
                }
            }

            // Inter-cluster edge
            List<Vector2Int> neighbors = clusterList.GetNeighborClusters(current.Index);
            foreach (var neighborCluster in neighbors)
            {
                Vector2Int? neighborEntrance = GetEntranceBetweenClusters(current.Index, neighborCluster, current.EntrancePos, unitRadius);
                if (neighborEntrance == null) continue;

                yield return (
                    new AbstractNode { Index = neighborCluster, EntrancePos = (Vector2Int)neighborEntrance },
                    current.Index.GetNeighborMoveCost(neighborCluster) // 경계 통과 비용
                );
            }
        }

        private List<Vector2Int> GetAllEntrances(Vector2Int Index, float unitRadius)
        {
            List<Vector2Int> entrances = GetList();
            entrances.Clear();

            Vector2Int[] directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (Vector2Int dir in directions)
            {
                foreach (var dirEntrance in clusterList.GetEntrances(Index, dir, unitRadius))
                {
                    if (dirEntrance != null)
                    {
                        entrances.Add(dirEntrance);
                    }
                }
            }
            return entrances;
        }

        private Vector2Int? GetEntranceBetweenClusters(Vector2Int from, Vector2Int to, Vector2Int currentEntrance, float unitRadius)
        {
            Vector2Int direction = to - from; // direction 정규화
            if (direction.x != 0) direction.x = direction.x > 0 ? 1 : -1;
            if (direction.y != 0) direction.y = direction.y > 0 ? 1 : -1;

            Vector2Int correspondingPos = currentEntrance + direction;
            List<Vector2Int> neighborEntrances = clusterList.GetEntrancesOnce(to, -direction, unitRadius);

            if (neighborEntrances != null && neighborEntrances.Contains(correspondingPos))
            {
                return correspondingPos;
            }

            return null;
        }

        private float CaculateHeuristic(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);
            // 대각선 이동 비용과 상하좌우 이동 비용을 각각 사용
            const float ORTHOGONAL_COST = 1f;
            const float DIAGONAL_COST = 1.4142f;
            return (Mathf.Min(dx, dy) * DIAGONAL_COST) + (Mathf.Abs(dx - dy) * ORTHOGONAL_COST);
        }

        private List<Vector2Int> GetList() => listPool.Count > 0 ? listPool.Pop() : new List<Vector2Int>();

        private bool IsWalkable(Vector2Int nodeIndex) => nodeList.Nodes[nodeIndex.x, nodeIndex.y].IsWalkable;

        private struct AbstractNode
        {
            public Vector2Int Index;
            public Vector2Int EntrancePos;

            public override readonly bool Equals(object obj)
            {
                if (obj is not AbstractNode other) return false;
                return Index == other.Index && EntrancePos == other.EntrancePos;
            }

            public override readonly int GetHashCode()
            {
                return Index.GetHashCode() ^ EntrancePos.GetHashCode();
            }
        }

        public struct ResultNode
        {
            public Vector2Int Index;
            public Vector2Int enteranceNode;
            public Vector2Int exitNode;

            public bool hasEntranceAndExit;
        }
    }
}
