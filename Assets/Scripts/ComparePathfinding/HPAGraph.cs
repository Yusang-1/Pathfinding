using UnityEngine;
using System.Collections.Generic;

public class HPAGraph
{
    private readonly Dictionary<float, Dictionary<Vector2Int, GraphNode>> nodesByUnitRadius = new();
    private readonly Dictionary<float, Dictionary<Vector2Int, List<EntranceData>>> entrancesDataByDirectionByRadius = new();
    private readonly Dictionary<(Vector2Int from, Vector2Int to, float radius), float> edgeCache = new();

    public class GraphNode : IPoolObject
    {
        public Vector2Int Position { get; private set; }
        public List<Vector2Int> Direction { get; private set; } // 인접 클러스터로의 방향
        public HashSet<Vector2Int> Neighbors { get; } = new(); // 인접 리스트
        public Dictionary<Vector2Int, float> EdgeWeights { get; } = new(10); // 간선 가중치

        public void SetData(Vector2Int position, Vector2Int direction)
        {
            Position = position;

            Direction ??= Vector2IntListPool.GetValue();
            Direction.Add(direction);
        }

        public void Clear()
        {
            Neighbors.Clear();
            EdgeWeights.Clear();
            Vector2IntListPool.ReleaseValue(Direction);
        }
    }

    public HPAGraph(Dictionary<UnitSize, float> unitRadiusDict)
    {
        foreach (var radius in unitRadiusDict.Values)
        {
            nodesByUnitRadius.Add(radius, new Dictionary<Vector2Int, GraphNode>());
        }
    }

    /// <summary> 노드(entrance) 추가 </summary>
    public bool TryAddNode(Vector2Int entrance, Vector2Int direction, NodeList nodeList, float unitRadius)
    {
        var nodes = nodesByUnitRadius[unitRadius];
        if (!nodes.ContainsKey(entrance))
        {
            nodes[entrance] = GraphNodePool.GetValue(entrance, direction);
            nodeList.NodeTypeController.SetNodeTypeInPathFinding(entrance, NodeType.entrance);
            return true;
        }
        else if (direction != Vector2Int.zero && !nodes[entrance].Direction.Contains(direction))
        {
            nodes[entrance].Direction.Add(direction);
            return true;
        }
        return false;
    }

    public bool TryAddEntranceNode(EntranceData entranceData, Vector2Int direction, NodeList nodeList, float unitRadius)
    {
        if (!entrancesDataByDirectionByRadius.ContainsKey(unitRadius))
        {
            entrancesDataByDirectionByRadius.Add(unitRadius, new Dictionary<Vector2Int, List<EntranceData>>());
        }
        var entrancesDataByDirection = entrancesDataByDirectionByRadius[unitRadius];
        entrancesDataByDirection ??= new Dictionary<Vector2Int, List<EntranceData>>();

        if (direction != Vector2Int.zero && !entrancesDataByDirection.ContainsKey(direction))
        {
            entrancesDataByDirection[direction] = new List<EntranceData>
            {
                entranceData
            };

            if (entranceData.LeftEntrance != entranceData.RightEntrance)
            {
                bool isLeftSuccess = TryAddNode(entranceData.LeftEntrance, direction, nodeList, unitRadius);
                bool isRightSuccess = TryAddNode(entranceData.RightEntrance, direction, nodeList, unitRadius);
                return isLeftSuccess && isRightSuccess;
            }
            else
            {
                bool isLeftSuccess = TryAddNode(entranceData.LeftEntrance, direction, nodeList, unitRadius);
                return isLeftSuccess;
            }
        }
        else if (direction != Vector2Int.zero)
        {
            entrancesDataByDirection[direction].Add(entranceData);

            if (entranceData.LeftEntrance != entranceData.RightEntrance)
            {
                bool isLeftSuccess = TryAddNode(entranceData.LeftEntrance, direction, nodeList, unitRadius);
                bool isRightSuccess = TryAddNode(entranceData.RightEntrance, direction, nodeList, unitRadius);
                return isLeftSuccess && isRightSuccess;
            }
            else
            {
                bool isLeftSuccess = TryAddNode(entranceData.LeftEntrance, direction, nodeList, unitRadius);
                return isLeftSuccess;
            }
        }
        else return false;
    }

    public void AddBidirectionalEdge(Vector2Int entrance1, Vector2Int entrance2, float weight, float unitRadius)
    {
        AddEdge(entrance1, entrance2, weight, unitRadius);
        AddEdge(entrance2, entrance1, weight, unitRadius);
    }

    private void AddEdge(Vector2Int from, Vector2Int to, float weight, float unitRadius)
    {
        Dictionary<Vector2Int, GraphNode> nodes = nodesByUnitRadius[unitRadius];
        if (!nodes.ContainsKey(from) || !nodes.ContainsKey(to)) return;

        var key = (from, to, unitRadius);
        if (!edgeCache.ContainsKey(key))
        {
            nodes[from].Neighbors.Add(to);
            nodes[from].EdgeWeights[to] = weight;
            edgeCache[key] = weight;
        }
    }

    public void RemoveTempNode(Vector2Int tempNode)
    {
        foreach (var nodes in nodesByUnitRadius.Values)
        {
            if (!nodes.ContainsKey(tempNode)) continue;

            GraphNodePool.ReleaseValue(nodes[tempNode]);
            nodes.Remove(tempNode);
        }

        List<Vector2Int> fromKeysToRemoveList = Vector2IntListPool.GetValue();
        List<Vector2Int> toKeysToRemoveList = Vector2IntListPool.GetValue();
        List<float> radiusKeysToRemoveList = FloatListPool.GetValue();

        foreach (var (from, to, radius) in edgeCache.Keys)
        {
            if (from == tempNode || to == tempNode)
            {
                fromKeysToRemoveList.Add(from);
                toKeysToRemoveList.Add(to);
                radiusKeysToRemoveList.Add(radius);
            }
        }

        for (int index = 0; index < fromKeysToRemoveList.Count; index++)
        {
            if (fromKeysToRemoveList[index] == tempNode || toKeysToRemoveList[index] == tempNode)
            {
                edgeCache.Remove((fromKeysToRemoveList[index], toKeysToRemoveList[index], radiusKeysToRemoveList[index]));
            }
        }

        Vector2IntListPool.ReleaseValue(fromKeysToRemoveList);
        Vector2IntListPool.ReleaseValue(toKeysToRemoveList);
        FloatListPool.ReleaseValue(radiusKeysToRemoveList);
    }

    /// <summary> 노드의 모든 이웃 노드 반환 </summary>
    public IEnumerable<Vector2Int> GetNeighbors(Vector2Int node, float unitRadius)
    {
        var nodes = nodesByUnitRadius[unitRadius];
        return nodes.ContainsKey(node) ? nodes[node].Neighbors : null;
    }

    /// <summary> 간선 가중치 조회 </summary>
    public bool TryGetEdgeWeight(Vector2Int from, Vector2Int to, out float weight, float unitRadius)
    {
        var nodes = nodesByUnitRadius[unitRadius];
        weight = 0;
        return nodes.ContainsKey(from) && nodes[from].EdgeWeights.TryGetValue(to, out weight);
    }

    /// <summary> 해당 방향의 모든 노드 반환 </summary>
    public IEnumerable<Vector2Int> GetNodesByDirection(Vector2Int direction, float unitRadius)
    {
        var nodes = nodesByUnitRadius[unitRadius];
        foreach (var pair in nodes)
        {
            var node = pair.Value;
            for (int i = 0; i < node.Direction.Count; i++)
            {
                if (node.Direction[i] == direction)
                {
                    yield return node.Position;
                }
            }
        }
    }
    public List<Vector2Int> GetNodesByDirectionOnce(Vector2Int direction, float unitRadius)
    {
        List<Vector2Int> temp = Vector2IntListPool.GetValue();

        foreach (var node in nodesByUnitRadius[unitRadius].Values)
        {
            for (int i = 0; i < node.Direction.Count; i++)
            {
                if (node.Direction[i] == direction)
                {
                    temp.Add(node.Position);
                }
            }
        }

        return temp;
    }

    public bool IsNodeConnected(Vector2Int node1, Vector2Int node2, float unitRadius)
    {
        return nodesByUnitRadius[unitRadius][node1].Neighbors.Contains(node2) || node1 == node2;
    }

    public void GetUsedEntrance(Vector2Int direction, Vector2Int entrance, out Vector2Int leftEntrance, out Vector2Int rightEntrance, float unitRadius)
    {
        if (direction == Vector2Int.zero)
        {
            leftEntrance = Vector2Int.zero;
            rightEntrance = Vector2Int.zero;
            Debug.LogWarning("direction이 zero");
            return;
        }

        List<EntranceData> datas = entrancesDataByDirectionByRadius[unitRadius][direction];
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i].HasEntrance(entrance))
            {
                JudgeLeftRight(datas[i], direction, out leftEntrance, out rightEntrance);
                return;
            }
        }

        leftEntrance = Vector2Int.zero;
        rightEntrance = Vector2Int.zero;
        Debug.LogWarning("direction방향의 entrance를 가진 EntranceData를 찾지 못함");
    }

    private void JudgeLeftRight(EntranceData data, Vector2Int direction, out Vector2Int leftEntrance, out Vector2Int rightEntrance)
    {
        var left = data.LeftEntrance;
        var right = data.RightEntrance;

        float dx = left.x - right.x;
        float dy = left.y - right.y;

        if (left == right)
        {
            leftEntrance = left;
            rightEntrance = right;
        }
        else if (direction == Vector2Int.up)
        {
            // x값이 큰 쪽이 right
            if (dx > 0)
            {
                rightEntrance = left;
                leftEntrance = right;
            }
            else
            {
                rightEntrance = right;
                leftEntrance = left;
            }
        }
        else if (direction == Vector2Int.down)
        {
            // x값이 작은 쪽이 right
            if (dx > 0)
            {
                rightEntrance = right;
                leftEntrance = left;
            }
            else
            {
                rightEntrance = left;
                leftEntrance = right;
            }
        }
        else if (direction == Vector2Int.left)
        {
            // y값이 큰 쪽이 right
            if (dy > 0)
            {
                rightEntrance = left;
                leftEntrance = right;
            }
            else
            {
                rightEntrance = right;
                leftEntrance = left;
            }
        }
        else // direction == Vector2Int.right
        {
            // y값이 작은 쪽이 right
            if (dy > 0)
            {
                rightEntrance = right;
                leftEntrance = left;
            }
            else
            {
                rightEntrance = left;
                leftEntrance = right;
            }
        }
    }

    public bool IsNodeInEntrance(Vector2Int nodeIndex, Vector2Int direction, float radius)
    {
        List<EntranceData> entranceDatas = entrancesDataByDirectionByRadius[radius][direction];
        foreach (var entranceData in entranceDatas)
        {
            if (entranceData.HasEntrance(nodeIndex))
            {
                return true;
            }
        }
        return false;
    }

    public struct EntranceData
    {
        public Vector2Int LeftEntrance;
        public Vector2Int RightEntrance;

        public readonly bool HasEntrance(Vector2Int entrance)
        {
            if (LeftEntrance == entrance || RightEntrance == entrance) return true;

            Vector2Int searchDirection;

            if (LeftEntrance.x == RightEntrance.x)
            {
                if (LeftEntrance.y < RightEntrance.y)
                {
                    searchDirection = new(0, 1);
                }
                else
                {
                    searchDirection = new(0, -1);
                }
            }
            else
            {
                if (LeftEntrance.x < RightEntrance.x)
                {
                    searchDirection = new(1, 0);
                }
                else
                {
                    searchDirection = new(-1, 0);
                }
            }

            Vector2Int compareVec = LeftEntrance + searchDirection;
            while (true)
            {
                if (entrance == compareVec) return true;

                compareVec += searchDirection;

                if (searchDirection.x == 0) // y축으로 이동
                {
                    if (searchDirection.y > 0 && compareVec.y >= RightEntrance.y) break;
                    else if (compareVec.y <= RightEntrance.y) break;
                }
                else
                {
                    if (searchDirection.x > 0 && compareVec.x >= RightEntrance.x) break;
                    else if (compareVec.x <= RightEntrance.x) break;
                }

            }

            return false;
        }
    }
}
