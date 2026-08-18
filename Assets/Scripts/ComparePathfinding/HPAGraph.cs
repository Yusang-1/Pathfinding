using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HPAGraph
{
    private readonly Dictionary<float, Dictionary<Vector2Int, GraphNode>> nodesByUnitRadius = new();
    private readonly Dictionary<float, Dictionary<Vector2Int, List<EntranceData>>> entrancesDataByDirectionByRadius = new();
    private readonly Dictionary<(Vector2Int from, Vector2Int to), float> edgeCache = new();

    private class GraphNode
    {
        public Vector2Int Position { get; }
        public List<Vector2Int> Direction { get; } // 인접 클러스터로의 방향
        public HashSet<Vector2Int> Neighbors { get; } = new(); // 인접 리스트
        public Dictionary<Vector2Int, float> EdgeWeights { get; } = new(); // 간선 가중치

        public GraphNode(Vector2Int position, Vector2Int direction)
        {
            Position = position;

            Direction ??= new List<Vector2Int>(3);
            Direction.Add(direction);
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
            nodes[entrance] = new GraphNode(entrance, direction);
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

    private void AddEdge(Vector2Int from, Vector2Int to, float weight, float unitRadius)
    {
        var nodes = nodesByUnitRadius[unitRadius];
        if (!nodes.ContainsKey(from) || !nodes.ContainsKey(to)) return;

        var key = (from, to);
        if (!edgeCache.ContainsKey(key))
        {
            nodes[from].Neighbors.Add(to);
            nodes[from].EdgeWeights[to] = weight;
            edgeCache[key] = weight;
        }
    }

    public void AddBidirectionalEdge(Vector2Int entrance1, Vector2Int entrance2, float weight, float unitRadius)
    {
        AddEdge(entrance1, entrance2, weight, unitRadius);
        AddEdge(entrance2, entrance1, weight, unitRadius);
    }

    public void RemoveTempNode(Vector2Int tempNode)
    {
        foreach (var nodes in nodesByUnitRadius.Values)
        {
            nodes.Remove(tempNode);
        }

        var keysToRemove = edgeCache.Keys.Where(k => k.from == tempNode || k.to == tempNode).ToList();

        foreach (var key in keysToRemove)
        {
            if (key.from == tempNode || key.to == tempNode)
            {
                edgeCache.Remove(key);
            }
        }
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
        foreach (var node in nodesByUnitRadius[unitRadius].Values)
        {
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
        List<Vector2Int> temp = new();
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

    // public struct EntranceData
    // {
    //     public Vector2Int LeftEntrance;
    //     public Vector2Int RightEntrance;

    //     public readonly bool HasEntrance(Vector2Int entrance)
    //     {
    //         return LeftEntrance == entrance || RightEntrance == entrance;
    //     }
    // }
}
