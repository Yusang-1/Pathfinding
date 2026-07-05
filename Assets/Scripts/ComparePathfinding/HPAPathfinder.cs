using UnityEngine;
using System.Collections.Generic;

public class HPAPathfinder
{
    private readonly HPAClusterList clusterList;
    private readonly NodeList nodeList;

    private readonly List<ClusterResult> results = new();

    // 메모리 풀: List 재사용으로 GC 감소
    private readonly Stack<List<Vector2Int>> listPool = new();
    private const int PoolSize = 10;

    private readonly PriorityQueue<int, float> openSet = new();
    private readonly HashSet<int> closedSet = new();
    private readonly Dictionary<int, AbstractNode> clusterDict = new();

    public HPAPathfinder(HPAClusterList clusterList, NodeList nodeList, AStarPathfinder lowLvPathfinder)
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
    public List<ClusterResult> FindClusterPath(Vector3 from, Vector3 to, out PathResult pathResult)
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
        clusterList.GetCluster(startCluster).AddNodeToGraph(startNode, nodeList);
        clusterList.GetCluster(goalCluster).AddNodeToGraph(goalNode, nodeList);
        nodeList.NodeTypeDrawer.ResetSearched();
        nodeList.NodeTypeDrawer.ResetTrace();

        List<ClusterResult> clusterPath;
        // from과 to가 같은 클러스터에 존재하고 startNode에서 goalNode로 이동 가능한 경우 resultNode하나 리턴
        if (startCluster == goalCluster && clusterList.GetCluster(startCluster).IsNodeConnected(startNode, goalNode))
        {
            return new() {
                new ClusterResult{
                    Index = startCluster, EnterDirection = Vector2Int.zero, ExitDirection = Vector2Int.zero // , HasEntranceAndExit = true
                }
            };
        }
        else
        {
            // 고수준 클러스터 경로 탐색
            clusterPath = FindAbstractClusterPath(startCluster, goalCluster, startNode, goalNode, out PathResult result);
            pathResult.AddResult(result);
        }

        if (clusterPath == null || clusterPath.Count == 0)
        {
            Debug.LogWarning("cluster경로를 찾지 못함");
        }

        // 시작지점과 도착지점을 clusterPath에 추가
        // var temp = clusterPath[0];
        // temp.enteranceNode = startNode;
        // temp.HasEntranceAndExit = true;
        // clusterPath[0] = temp;

        // temp = clusterPath[^1];
        // temp.exitNode = goalNode;
        // temp.HasEntranceAndExit = true;
        // clusterPath[^1] = temp;

        // start cluster, goal cluster에 추가된 노드 제거
        clusterList.GetCluster(startCluster).RemoveTempNodeInGraph();
        clusterList.GetCluster(goalCluster).RemoveTempNodeInGraph();

        return clusterPath;
    }

    /// <summary> 고수준 클러스터 경로 탐색 </summary>    
    private List<ClusterResult> FindAbstractClusterPath(Vector2Int startClusterIndex, Vector2Int goalClusterIndex, Vector2Int startNode, Vector2Int goalNode, out PathResult pathResult)
    {
        openSet.Clear();
        closedSet.Clear();
        clusterDict.Clear();

        pathResult = new();
        List<Vector2Int> startEntrances = GetAllEntrances(startClusterIndex);
        if (startEntrances == null || startEntrances.Count == 0) return null;

        AbstractNode startVirtual = new()
        {
            ClusterIndex = startClusterIndex,
            EntranceNodeIndex = startNode,
            G = 0,
            H = 0
        };
        int startHash = startVirtual.GetHashCode();
        openSet.Enqueue(startHash, 0);
        clusterDict.Add(startHash, startVirtual);

        while (openSet.Count > 0)
        {
            int currentClusterHash = openSet.Dequeue();
            Vector2Int currentClusterIndex = clusterDict[currentClusterHash].ClusterIndex;

            // 목적지 도착
            if (currentClusterIndex == goalClusterIndex
                && clusterList.GetCluster(currentClusterIndex).IsNodeConnected(clusterDict[currentClusterHash].EntranceNodeIndex, goalNode))
            {
                pathResult.MemoryUsed += openSet.Capacity;
                pathResult.MemoryUsed += clusterDict.Count;
                pathResult.MemoryUsed += closedSet.Count;
                return ReconstructAbstractPath(clusterDict, currentClusterHash, startHash);
            }

            if (closedSet.Contains(currentClusterHash)) continue;
            closedSet.Add(currentClusterHash);

            foreach (var (neighborCluster, cost) in GetAbstractNeighbors(clusterDict[currentClusterHash]))
            {
                int neighborClusterHash = neighborCluster.GetHashCode();

                if (closedSet.Contains(neighborClusterHash)) continue;

                pathResult.SearchedCount++;
                float tentativeG = clusterDict[currentClusterHash].G + cost;

                if (!clusterDict.ContainsKey(neighborClusterHash) || tentativeG < neighborCluster.G)
                {
                    if (!clusterDict.ContainsKey(neighborClusterHash))
                    {
                        AbstractNode newCluster = neighborCluster;
                        newCluster.H = CaculateHeuristic(neighborCluster.ClusterIndex, goalClusterIndex);
                        clusterDict.Add(neighborClusterHash, newCluster);
                    }
                    AbstractNode cluster = clusterDict[neighborClusterHash];
                    cluster.G = tentativeG;
                    cluster.ParentClusterHash = clusterDict[currentClusterHash].GetHashCode();
                    clusterDict[neighborClusterHash] = cluster;

                    openSet.Enqueue(neighborClusterHash, clusterDict[neighborClusterHash].F);
                }
            }
        }

        return null; // 경로 없음
    }

    private List<ClusterResult> ReconstructAbstractPath(Dictionary<int, AbstractNode> clusterDict, int current, int start)
    {
        results.Clear();

        if (clusterDict == null || clusterDict.Count <= 1)
        {
            results.Add(new ClusterResult
            {
                Index = clusterDict[current].ClusterIndex,
                EnterDirection = Vector2Int.zero,
                ExitDirection = Vector2Int.zero
            });
            return results;
        }

        AbstractNode childCluster = clusterDict[current];
        AbstractNode currentCluster = clusterDict[childCluster.ParentClusterHash];
        AbstractNode parentCluster;

        // 도착지 노드 세팅
        results.Add(new ClusterResult
        {
            Index = childCluster.ClusterIndex,
            EnterDirection = currentCluster.ClusterIndex - childCluster.ClusterIndex,
            ExitDirection = Vector2Int.zero,
            EntranceExit = Vector2Int.zero
        });

        Vector2Int startClusterIndex = childCluster.ClusterIndex;
        Vector2Int startClusterExitDirection = Vector2Int.zero;
        Vector2Int startEntranceExit = Vector2Int.zero;
        
        while (true)
        {
            int childHash = current;
            childCluster = clusterDict[childHash];

            int currentHash = childCluster.ParentClusterHash;
            currentCluster = clusterDict[currentHash];
            if (currentHash == start) break;

            int parentHash = currentCluster.ParentClusterHash;
            parentCluster = clusterDict[parentHash];
            if (parentHash == start)
            {
                startClusterIndex = parentCluster.ClusterIndex;
                startClusterExitDirection = parentCluster.ClusterIndex == currentCluster.ClusterIndex
                                            ? childCluster.ClusterIndex - parentCluster.ClusterIndex
                                            : currentCluster.ClusterIndex - parentCluster.ClusterIndex;
                startEntranceExit = parentCluster.ClusterIndex == currentCluster.ClusterIndex
                                    ? currentCluster.EntranceNodeIndex
                                    : currentCluster.EntranceNodeIndex + startClusterExitDirection;
                break;
            }

            if (parentCluster.ClusterIndex == currentCluster.ClusterIndex) // 입구와 출구 모두 따로 있는 경우 (current와 parent의 ClusterIndex가 같음)
            {
                int grandparentHash = parentCluster.ParentClusterHash;
                AbstractNode grandparentCluster = clusterDict[grandparentHash];
                
                // grandparent -> parent -> curent -> child
                results.Add(new ClusterResult
                {
                    Index = currentCluster.ClusterIndex,
                    EnterDirection = grandparentCluster.ClusterIndex - currentCluster.ClusterIndex,
                    ExitDirection = childCluster.ClusterIndex - currentCluster.ClusterIndex,
                    EntranceExit = currentCluster.EntranceNodeIndex
                });

                if (grandparentHash == start)
                {
                    startClusterIndex = grandparentCluster.ClusterIndex;
                    startClusterExitDirection = parentCluster.ClusterIndex - startClusterIndex;
                    startEntranceExit = parentCluster.EntranceNodeIndex + startClusterExitDirection;
                    break;
                }

                current = parentHash;
            }
            else // 입구에서 바로 출구로 나간 경우 (current와 parent의 ClusterIndex가 다름)
            {
                // parent -> curent -> child
                results.Add(new ClusterResult
                {
                    Index = currentCluster.ClusterIndex,
                    EnterDirection = parentCluster.ClusterIndex - currentCluster.ClusterIndex,
                    ExitDirection = childCluster.ClusterIndex - currentCluster.ClusterIndex,
                    EntranceExit = currentCluster.EntranceNodeIndex
                });

                if (parentHash == start)
                {
                    startClusterIndex = parentCluster.ClusterIndex;
                    startClusterExitDirection = parentCluster.ClusterIndex - startClusterIndex;
                    startEntranceExit = currentCluster.EntranceNodeIndex + startClusterExitDirection;
                    break;
                }

                current = currentHash;
            }
        }

        // 출발지 노드 세팅
        results.Add(new ClusterResult
        {
            Index = startClusterIndex,
            EnterDirection = Vector2Int.zero,
            ExitDirection = startClusterExitDirection,
            EntranceExit = startEntranceExit
        });

        results.Reverse();
        return results;
    }

    private IEnumerable<(AbstractNode index, float cost)> GetAbstractNeighbors(AbstractNode current)
    {
        var cluster = clusterList.GetCluster(current.ClusterIndex);

        // Intra-cluster edges
        List<Vector2Int> entranceList = GetAllEntrances(current.ClusterIndex);
        foreach (var other in entranceList)
        {
            if (other == current.EntranceNodeIndex) continue;

            if (cluster.TryGetIntraEdgeCost(current.EntranceNodeIndex, other, out float intraCost))
            {
                yield return (
                    new AbstractNode { ClusterIndex = current.ClusterIndex, EntranceNodeIndex = other },
                    intraCost
                );
            }
        }

        // Inter-cluster edge
        List<Vector2Int> neighbors = clusterList.GetNeighborClusters(current.ClusterIndex);
        foreach (var neighborCluster in neighbors)
        {
            Vector2Int? neighborEntrance = GetEntranceBetweenClusters(current.ClusterIndex, neighborCluster, current.EntranceNodeIndex);
            if (neighborEntrance == null) continue;

            yield return (
                new AbstractNode { ClusterIndex = neighborCluster, EntranceNodeIndex = (Vector2Int)neighborEntrance },
                current.ClusterIndex.GetNeighborMoveCost(neighborCluster) // 경계 통과 비용
            );
        }
    }

    private List<Vector2Int> GetAllEntrances(Vector2Int Index)
    {
        List<Vector2Int> entrances = GetList();
        entrances.Clear();

        Vector2Int[] directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in directions)
        {
            foreach (var dirEntrance in clusterList.GetEntrances(Index, dir))
            {
                if (dirEntrance != null)
                {
                    entrances.Add(dirEntrance);
                }
            }
        }
        return entrances;
    }

    private Vector2Int? GetEntranceBetweenClusters(Vector2Int from, Vector2Int to, Vector2Int currentEntrance)
    {
        Vector2Int direction = to - from; // direction 정규화
        if (direction.x != 0) direction.x = direction.x > 0 ? 1 : -1;
        if (direction.y != 0) direction.y = direction.y > 0 ? 1 : -1;

        Vector2Int correspondingPos = currentEntrance + direction;
        List<Vector2Int> neighborEntrances = clusterList.GetEntrancesOnce(to, -direction);

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

        return dx + dy;
    }

    private List<Vector2Int> GetList() => listPool.Count > 0 ? listPool.Pop() : new List<Vector2Int>();

    private bool IsWalkable(Vector2Int nodeIndex) => nodeList.Nodes[nodeIndex.x, nodeIndex.y].IsWalkable;

    private struct AbstractNode
    {
        public Vector2Int ClusterIndex;
        public int ParentClusterHash;
        public Vector2Int EntranceNodeIndex;
        public float G;
        public float H;
        public readonly float F => G + H;

        public override readonly bool Equals(object obj)
        {
            if (obj is not AbstractNode other) return false;
            return ClusterIndex == other.ClusterIndex && EntranceNodeIndex == other.EntranceNodeIndex;
        }

        public override readonly int GetHashCode()
        {
            return ClusterIndex.GetHashCode() ^ EntranceNodeIndex.GetHashCode();
        }
    }

    public struct ClusterResult
    {
        public Vector2Int Index;
        public Vector2Int EnterDirection;
        public Vector2Int ExitDirection;
        public Vector2Int EntranceExit;
    }
}
