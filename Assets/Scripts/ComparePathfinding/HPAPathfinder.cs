using UnityEngine;
using System.Collections.Generic;

public class HPAPathfinder
{
    private readonly HPAClusterList clusterList;
    private readonly NodeList nodeList;

    private readonly List<ResultNode> results = new();

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
    public List<ResultNode> FindClusterPath(Vector3 from, Vector3 to, out PathResult pathResult)
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
        nodeList.NodeInfo.ResetSearched();
        nodeList.NodeInfo.ResetTrace();

        List<ResultNode> clusterPath;
        // from과 to가 같은 클러스터에 존재하고 startNode에서 goalNode로 이동 가능한 경우 resultNode하나 리턴
        if (startCluster == goalCluster && clusterList.GetCluster(startCluster).IsNodeConnected(startNode, goalNode))
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
            clusterPath = FindAbstractClusterPath(startCluster, goalCluster, startNode, goalNode, out PathResult result);
            pathResult.AddResult(result);
        }

        if (clusterPath == null || clusterPath.Count == 0)
        {
            Debug.LogWarning("cluster경로를 찾지 못함");
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
    private List<ResultNode> FindAbstractClusterPath(Vector2Int startClusterIndex, Vector2Int goalClusterIndex, Vector2Int startNode, Vector2Int goalNode, out PathResult pathResult)
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

                    var testHash = clusterDict[neighborClusterHash].GetHashCode();

                    openSet.Enqueue(neighborClusterHash, clusterDict[neighborClusterHash].F);
                }
            }
        }

        return null; // 경로 없음
    }

    private List<ResultNode> ReconstructAbstractPath(Dictionary<int, AbstractNode> clusterDict, int current, int start)
    {
        var nodes = new Dictionary<Vector2Int, int>();
        AbstractNode currentCluster = clusterDict[current];

        // 도착지 노드 세팅 (current는 목표 노드)
        results.Add(new ResultNode
        {
            Index = currentCluster.ClusterIndex,
            enteranceNode = currentCluster.EntranceNodeIndex,
            hasEntranceAndExit = true
        });

        nodes.Add(currentCluster.ClusterIndex, results.Count - 1);

        current = currentCluster.ParentClusterHash;
        currentCluster = clusterDict[current];

        ResultNode temp;
        while (!current.Equals(start))
        {
            if (nodes.ContainsKey(currentCluster.ClusterIndex))
            {
                if (!results[nodes[currentCluster.ClusterIndex]].hasEntranceAndExit)
                {
                    temp = results[nodes[currentCluster.ClusterIndex]];
                    temp.enteranceNode = currentCluster.EntranceNodeIndex;
                    temp.hasEntranceAndExit = true;
                    results[nodes[currentCluster.ClusterIndex]] = temp;
                }
                else
                {
                    results.Add(new ResultNode
                    {
                        Index = currentCluster.ClusterIndex,
                        exitNode = currentCluster.EntranceNodeIndex,
                    });
                    nodes[currentCluster.ClusterIndex] = results.Count - 1;
                }
            }
            else
            {
                results.Add(new ResultNode
                {
                    Index = currentCluster.ClusterIndex,
                    exitNode = currentCluster.EntranceNodeIndex,
                });

                nodes.Add(currentCluster.ClusterIndex, results.Count - 1);
            }

            current = currentCluster.ParentClusterHash;
            currentCluster = clusterDict[current];
        }

        if (results.Count == 1) // 도착지 노드만 세팅된 경우 출발지 노드를 별도로 세팅
        {
            results.Add(new ResultNode
            {
                Index = currentCluster.ClusterIndex,
                exitNode = currentCluster.EntranceNodeIndex,
                hasEntranceAndExit = true
            });
        }

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
        // 대각선 이동 비용과 상하좌우 이동 비용을 각각 사용
        // const float ORTHOGONAL_COST = 1f;
        // const float DIAGONAL_COST = 1.4142f;
        // return (Mathf.Min(dx, dy) * DIAGONAL_COST) + (Mathf.Abs(dx - dy) * ORTHOGONAL_COST);
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

    public struct ResultNode
    {
        public Vector2Int Index;
        public Vector2Int enteranceNode;
        public Vector2Int exitNode;

        public bool hasEntranceAndExit;
    }
}
