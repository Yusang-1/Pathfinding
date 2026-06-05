using UnityEngine;
using System.Collections.Generic;

public class HPAPathfinder
{
    private readonly int clusterSize;
    private readonly HPAClusterList clusterList;
    private readonly NodeList nodeList;
    private readonly AStarPathfinder lowLvPathfinder;

    private readonly Dictionary<(Vector2Int, Vector2Int), List<Vector3>> pathCache = new();
    private readonly List<ResultNode> results = new();

    // 메모리 풀: List 재사용으로 GC 감소
    private readonly Stack<List<Vector2Int>> listPool = new();
    private const int PoolSize = 10;
    private int numberOfNodesSearched;

    public HPAPathfinder(int clusterSize, HPAClusterList clusterList, NodeList nodeList, AStarPathfinder lowLvPathfinder)
    {
        this.clusterSize = clusterSize;
        this.clusterList = clusterList;
        this.nodeList = nodeList;
        this.lowLvPathfinder = lowLvPathfinder;

        // 메모리 풀 초기화
        for (int i = 0; i < PoolSize; i++)
        {
            listPool.Push(new List<Vector2Int>());
        }
    }


    /// <summary> high level cluster 경로를 반환 </summary>
    public List<ResultNode> FindClusterPath(Vector3 from, Vector3 to)
    {
        results.Clear();
        Vector2Int startNode = nodeList.GetNodeIndex(from);
        Vector2Int goalNode = nodeList.GetNodeIndex(to);

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

        List<ResultNode> clusterPath;
        // from과 to가 같은 클러스터에 존재할 경우 저수준 경로만 반환
        if (startCluster == goalCluster)
        {
            // startNode에서 goalNode로 이동 가능한지 판별
            if (clusterList.GetCluster(startCluster).IsNodeConnected(startNode, goalNode))
            {
                clusterList.SetClusterActive(startCluster, true);
                // 가능하면 resultNode하나 리턴
                return new() {
                    new ResultNode{
                        ClusterIndex = startCluster, enteranceNode = startNode, exitNode = goalNode, hasEntranceAndExit = true
                    }
                };
            }
            else
            {
                // 고수준 클러스터 경로 탐색
                clusterPath = FindAbstractClusterPath(startCluster, goalCluster, startNode, goalNode);
            }
        }
        else
        {
            // 고수준 클러스터 경로 탐색
            clusterPath = FindAbstractClusterPath(startCluster, goalCluster, startNode, goalNode);
        }

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

    /// <summary> cluster 경로 -> 전체 좌표 경로 변환 </summary>
    private List<Vector3> BuildFullPath(Vector2Int startNode, Vector2Int goalNode, List<Vector2Int> clusterPath)
    {
        List<Vector3> fullPath = new() { nodeList.GridToWorld(startNode) };

        for (int i = 0; i < clusterPath.Count - 1; i++)
        {
            Vector2Int from = clusterPath[i];
            Vector2Int to = clusterPath[i + 1];

            // 캐시 확인
            if (pathCache.TryGetValue((from, to), out var cachedPath))
            {
                // 마지막 노드 제외 (중복 방지)
                for (int j = 0; j < cachedPath.Count - 1; j++)
                {
                    fullPath.Add(cachedPath[j]);
                }
            }
            else
            {
                // 저수준 경로 계산 및 캐싱
                List<Vector3> segmentPath = new();
                segmentPath = lowLvPathfinder.FindPath(
                    nodeList.GridToWorld(from),
                    nodeList.GridToWorld(to)
                );

                if (segmentPath != null && segmentPath.Count > 0)
                {
                    pathCache[(from, to)] = segmentPath;
                    for (int j = 0; j < segmentPath.Count - 1; j++)
                    {
                        fullPath.Add(segmentPath[j]);
                    }
                }
            }
        }

        // 목표점 추가
        fullPath.Add(nodeList.GridToWorld(goalNode));

        return fullPath;
    }

    /// <summary> 고수준 클러스터 경로 탐색 </summary>    
    private List<ResultNode> FindAbstractClusterPath(Vector2Int startCluster, Vector2Int goalCluster, Vector2Int startNode, Vector2Int goalNode)
    {
        PriorityQueue<AbstractNode, float> openSet = new();
        Dictionary<AbstractNode, AbstractNode> cameFrom = new();
        Dictionary<AbstractNode, float> gCost = new();
        Dictionary<AbstractNode, float> fCost = new();
        HashSet<AbstractNode> closedSet = new();

        List<Vector2Int> startEntrances = GetAllEntrances(startCluster);
        if (startEntrances == null || startEntrances.Count == 0) return null;

        AbstractNode startVirtual = new()
        {
            ClusterIndex = startCluster,
            EntrancePos = startNode
        };
        gCost.Add(startVirtual, 0);
        fCost.Add(startVirtual, Heuristic(startNode, goalNode));
        openSet.Enqueue(startVirtual, fCost[startVirtual]);

        while (openSet.Count > 0)
        {
            AbstractNode current = openSet.Dequeue();
            if (current.ClusterIndex == goalCluster && clusterList.GetCluster(current.ClusterIndex).IsNodeConnected(current.EntrancePos, goalNode))
            {
                Debug.Log($"HPA*의 탐색한 노드 수 : {numberOfNodesSearched}");
                return ReconstructAbstractPath(cameFrom, current, startVirtual);
            }
            if (closedSet.Contains(current)) continue;
            closedSet.Add(current);

            foreach (var (neighbor, cost) in GetAbstractNeighbors(current, startCluster))
            {
                if (closedSet.Contains(neighbor)) continue;
                numberOfNodesSearched++;
                
                float tentativeG = gCost[current] + cost;

                if (!gCost.ContainsKey(neighbor) || tentativeG < gCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeG;
                    fCost[neighbor] = tentativeG + Heuristic(neighbor.EntrancePos, goalNode);

                    openSet.Enqueue(neighbor, fCost[neighbor]);
                }
            }
        }

        return null; // 경로 없음
    }

    private List<ResultNode> ReconstructAbstractPath(Dictionary<AbstractNode, AbstractNode> cameFrom, AbstractNode current, AbstractNode start)
    {
        var nodes = new Dictionary<Vector2Int, int>();

        // 도착지 노드 세팅
        //clusterList.SetClusterActive(current.ClusterIndex, true);
        results.Add(new ResultNode
        {
            ClusterIndex = current.ClusterIndex,
            enteranceNode = current.EntrancePos,
            hasEntranceAndExit = true
        });

        nodes.Add(current.ClusterIndex, results.Count - 1);
        nodeList.SetNodeTypeInPathFinding(current.EntrancePos, NodeType.entranceUsed);
        current = cameFrom[current];

        ResultNode temp;
        while (!current.Equals(start))
        {
            if (nodes.ContainsKey(current.ClusterIndex))
            {
                if (!results[nodes[current.ClusterIndex]].hasEntranceAndExit)
                {
                    temp = results[nodes[current.ClusterIndex]];
                    temp.enteranceNode = current.EntrancePos;
                    temp.hasEntranceAndExit = true;
                    results[nodes[current.ClusterIndex]] = temp;

                    nodeList.SetNodeTypeInPathFinding(current.EntrancePos, NodeType.entranceUsed);
                }
                else
                {
                    results.Add(new ResultNode
                    {
                        ClusterIndex = current.ClusterIndex,
                        exitNode = current.EntrancePos,
                    });
                    nodes[current.ClusterIndex] = results.Count - 1;
                    nodeList.SetNodeTypeInPathFinding(current.EntrancePos, NodeType.entranceUsed);
                }
            }
            else
            {
                results.Add(new ResultNode
                {
                    ClusterIndex = current.ClusterIndex,
                    exitNode = current.EntrancePos,
                });

                nodes.Add(current.ClusterIndex, results.Count - 1);
                nodeList.SetNodeTypeInPathFinding(current.EntrancePos, NodeType.entranceUsed);
            }

            current = cameFrom[current];
        }

        results.Reverse();
        return results;
    }

    private IEnumerable<(AbstractNode node, float cost)> GetAbstractNeighbors(AbstractNode current, Vector2Int startCluster)
    {
        var cluster = clusterList.GetCluster(current.ClusterIndex);

        // Intra-cluster edges
        List<Vector2Int> entranceList = GetAllEntrances(current.ClusterIndex);
        foreach (var other in entranceList)
        {
            if (other == current.EntrancePos) continue;

            if (cluster.TryGetIntraEdgeCost(current.EntrancePos, other, out float intraCost))
            {
                yield return (
                    new AbstractNode { ClusterIndex = current.ClusterIndex, EntrancePos = other },
                    intraCost
                );
            }
        }

        // Inter-cluster edge
        List<Vector2Int> neighbors = clusterList.GetNeighborClusters(current.ClusterIndex);
        foreach (var neighborCluster in neighbors)
        {
            Vector2Int? neighborEntrance = GetEntranceBetweenClusters(current.ClusterIndex, neighborCluster, current.EntrancePos);
            if (neighborEntrance == null) continue;

            yield return (
                new AbstractNode { ClusterIndex = neighborCluster, EntrancePos = (Vector2Int)neighborEntrance },
                1f // 경계 통과 비용
            );
        }
    }

    private List<Vector2Int> GetAllEntrances(Vector2Int clusterIndex)
    {
        List<Vector2Int> entrances = GetList();
        entrances.Clear();

        Vector2Int[] directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in directions)
        {
            foreach (var dirEntrance in clusterList.GetEntrances(clusterIndex, dir))
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

    private float Heuristic(Vector2Int from, Vector2Int to) => (Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y)) * clusterSize;

    private List<Vector2Int> GetList() => listPool.Count > 0 ? listPool.Pop() : new List<Vector2Int>();

    private bool IsWalkable(Vector2Int nodeIndex) => nodeList.Nodes[nodeIndex.x, nodeIndex.y].IsWalkable;

    private struct AbstractNode
    {
        public Vector2Int ClusterIndex;
        public Vector2Int EntrancePos;

        public override readonly bool Equals(object obj)
        {
            if (obj is not AbstractNode other) return false;
            return ClusterIndex == other.ClusterIndex && EntrancePos == other.EntrancePos;
        }

        public override readonly int GetHashCode()
        {
            return ClusterIndex.GetHashCode() ^ EntrancePos.GetHashCode();
        }
    }

    public struct ResultNode
    {
        public Vector2Int ClusterIndex;
        public Vector2Int enteranceNode;
        public Vector2Int exitNode;

        public bool hasEntranceAndExit;
    }
}
