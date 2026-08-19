using UnityEngine;
using System.Collections.Generic;

public class HPAPathfinder
{
    private readonly HPAClusterList clusterList;
    private readonly NodeList nodeList;
    
    private readonly List<NewClusterResult> newResults = new();

    private readonly PriorityQueue<int, float> openSet = new();
    private readonly HashSet<int> closedSet = new();
    private readonly Dictionary<int, AbstractNode> clusterDict = new();

    public HPAPathfinder(HPAClusterList clusterList, NodeList nodeList)
    {
        this.clusterList = clusterList;
        this.nodeList = nodeList;
    }

    /// <summary> high level cluster 경로를 반환 </summary>
    public ClusterResultWrapper FindClusterPath(ClusterResultWrapper clusterResultWrapper)
    {
        Vector3 from = clusterResultWrapper.From;
        Vector3 to = clusterResultWrapper.To;
        float unitRadius = clusterResultWrapper.UnitRadius;

        newResults.Clear();
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
        clusterList.GetCluster(startCluster).AddNodeToGraph(startNode, nodeList, clusterList, unitRadius);
        clusterList.GetCluster(goalCluster).AddNodeToGraph(goalNode, nodeList, clusterList, unitRadius);
        nodeList.NodeTypeController.NodeTypeDrawer.ResetSearched();
        nodeList.NodeTypeController.NodeTypeDrawer.ResetTrace();

        List<NewClusterResult> clusterPath;
        // from과 to가 같은 클러스터에 존재하고 startNode에서 goalNode로 이동 가능한 경우 resultNode하나 리턴
        if (startCluster == goalCluster && clusterList.GetCluster(startCluster).IsNodeConnected(startNode, goalNode, unitRadius))
        {
            // var result = ClusterResultPool.GetValue();
            var result = new NewClusterResult()
            {
                Index = startCluster,
                EnterDirection = Vector2Int.zero,
                ExitDirection = Vector2Int.zero,
                EntranceExit = goalNode,
            };

            clusterResultWrapper.SetClusterResult(result);
            return clusterResultWrapper;
        }
        else
        {
            // 고수준 클러스터 경로 탐색
            clusterPath = FindAbstractClusterPath(startCluster, goalCluster, startNode, goalNode, unitRadius);
        }

        if (clusterPath == null || clusterPath.Count == 0)
        {
            Debug.LogWarning("cluster경로를 찾지 못함");
        }

        // start cluster, goal cluster에 추가된 노드 제거
        clusterList.GetCluster(startCluster).RemoveTempNodeInGraph();
        clusterList.GetCluster(goalCluster).RemoveTempNodeInGraph();

        clusterResultWrapper.SetClusterResult(clusterPath);
        return clusterResultWrapper;
    }

    /// <summary> 고수준 클러스터 경로 탐색 </summary>    
    private List<NewClusterResult> FindAbstractClusterPath(Vector2Int startClusterIndex, Vector2Int goalClusterIndex, Vector2Int startNode, Vector2Int goalNode, float unitRadius)
    {
        openSet.Clear();
        closedSet.Clear();
        clusterDict.Clear();

        List<Vector2Int> startEntrances = GetAllEntrances(startClusterIndex, unitRadius);
        if (startEntrances == null || startEntrances.Count == 0) return null;
        Vector2IntListPool.ReleaseValue(startEntrances);

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
                && clusterList.GetCluster(currentClusterIndex).IsNodeConnected(clusterDict[currentClusterHash].EntranceNodeIndex, goalNode, unitRadius))
            {
                PathResultRecorder.AddMemoryUsed(openSet.Capacity + clusterDict.Count + closedSet.Count);

                var results = ReconstructAbstractPath(clusterDict, currentClusterHash, startHash, startNode, goalNode);
                return results;
            }

            if (closedSet.Contains(currentClusterHash)) continue;
            closedSet.Add(currentClusterHash);

            foreach (var (neighborCluster, cost) in GetAbstractNeighbors(clusterDict[currentClusterHash], unitRadius))
            {
                int neighborClusterHash = neighborCluster.GetHashCode();

                if (closedSet.Contains(neighborClusterHash)) continue;

                PathResultRecorder.AddSearchedCount();

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

    private List<NewClusterResult> ReconstructAbstractPath(Dictionary<int, AbstractNode> clusterDict, int current, int start, Vector2Int startNode, Vector2Int goalNode)
    {
        newResults.Clear();
        NewClusterResult result;

        if (clusterDict == null || clusterDict.Count <= 1)
        {
            result = new NewClusterResult()
            {
                Index = clusterDict[current].ClusterIndex,
                EnterDirection = Vector2Int.zero,
                ExitDirection = Vector2Int.zero,
                EntranceExit = Vector2Int.zero
            };
            newResults.Add(result);
            return newResults;
        }

        AbstractNode childCluster = clusterDict[current];
        AbstractNode currentCluster = clusterDict[childCluster.ParentClusterHash];
        AbstractNode parentCluster;

        // 도착지 노드 세팅
        result = new NewClusterResult()
        {
            Index = childCluster.ClusterIndex,
            EnterDirection = currentCluster.ClusterIndex - childCluster.ClusterIndex,
            ExitDirection = Vector2Int.zero,
            EntranceExit = goalNode
        };
        // EntranceEnter = goalNodeEnter        
        newResults.Add(result);

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
                result = new NewClusterResult()
                {
                    Index = currentCluster.ClusterIndex,
                    EnterDirection = grandparentCluster.ClusterIndex - currentCluster.ClusterIndex,
                    ExitDirection = childCluster.ClusterIndex - currentCluster.ClusterIndex,
                    EntranceExit = currentCluster.EntranceNodeIndex
                };
                // EntranceEnter = parentCluster.EntranceNodeIndex                
                newResults.Add(result);

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
                result = new NewClusterResult()
                {
                    Index = currentCluster.ClusterIndex,
                    EnterDirection = parentCluster.ClusterIndex - currentCluster.ClusterIndex,
                    ExitDirection = childCluster.ClusterIndex - currentCluster.ClusterIndex,
                    EntranceExit = currentCluster.EntranceNodeIndex
                };
                // EntranceEnter = currentCluster.EntranceNodeIndex                
                newResults.Add(result);

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
        result = new NewClusterResult()
        {
            Index = startClusterIndex,
            EnterDirection = Vector2Int.zero,
            ExitDirection = startClusterExitDirection,
            EntranceExit = startEntranceExit
        };
        // EntranceEnter = startNode        
        newResults.Add(result);

        newResults.Reverse();
        return newResults;
    }

    private IEnumerable<(AbstractNode index, float cost)> GetAbstractNeighbors(AbstractNode current, float unitRadius)
    {
        var cluster = clusterList.GetCluster(current.ClusterIndex);

        // Intra-cluster edges
        List<Vector2Int> entranceList = GetAllEntrances(current.ClusterIndex, unitRadius);
        foreach (var other in entranceList)
        {
            if (other == current.EntranceNodeIndex) continue;

            if (cluster.TryGetIntraEdgeCost(current.EntranceNodeIndex, other, out float intraCost, unitRadius))
            {
                yield return (
                    new AbstractNode { ClusterIndex = current.ClusterIndex, EntranceNodeIndex = other },
                    intraCost
                );
            }
        }
        Vector2IntListPool.ReleaseValue(entranceList);

        // Inter-cluster edge
        List<Vector2Int> neighbors = clusterList.GetNeighborClusters(current.ClusterIndex);
        foreach (var neighborCluster in neighbors)
        {
            Vector2Int? neighborEntrance = GetEntranceBetweenClusters(current.ClusterIndex, neighborCluster, current.EntranceNodeIndex, unitRadius);
            if (neighborEntrance == null) continue;

            yield return (
                new AbstractNode { ClusterIndex = neighborCluster, EntranceNodeIndex = (Vector2Int)neighborEntrance },
                current.ClusterIndex.GetNeighborMoveCost(neighborCluster) // 경계 통과 비용
            );
        }
    }
    
    private readonly Vector2Int[] directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    private List<Vector2Int> GetAllEntrances(Vector2Int Index, float unitRadius)
    {
        List<Vector2Int> entrances = Vector2IntListPool.GetValue();
        entrances.Clear();
        
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

        return dx + dy;
    }

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
}

public struct NewClusterResult
{
    public Vector2Int Index;
    public Vector2Int EnterDirection;
    public Vector2Int ExitDirection;
    public Vector2Int EntranceExit;
}
