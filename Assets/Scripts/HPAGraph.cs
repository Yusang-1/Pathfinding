using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HPAGraph
{
    private readonly Dictionary<Vector2Int, GraphNode> nodes = new();
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

            Direction ??= new List<Vector2Int>(2);
            Direction.Add(direction);
        }
    }

    /// <summary> 노드(entrance) 추가 </summary>
    public void AddNode(Vector2Int entrance, Vector2Int direction, NodeList nodeList)
    {
        if (!nodes.ContainsKey(entrance))
        {
            nodes[entrance] = new GraphNode(entrance, direction);
            nodeList.SetNodeTypeInPathFinding(entrance, NodeType.entrance);
        }
        else if(!nodes[entrance].Direction.Contains(direction))
        {
            nodes[entrance].Direction.Add(direction);
        }
    }

    public void AddEdge(Vector2Int from, Vector2Int to, float weight)
    {
        if (!nodes.ContainsKey(from) || !nodes.ContainsKey(to)) return;

        var key = (from, to);
        if (!edgeCache.ContainsKey(key))
        {
            nodes[from].Neighbors.Add(to);
            nodes[from].EdgeWeights[to] = weight;
            edgeCache[key] = weight;
        }
    }

    public void AddBidirectionalEdge(Vector2Int entrance1, Vector2Int entrance2, float weight)
    {
        AddEdge(entrance1, entrance2, weight);
        AddEdge(entrance2, entrance1, weight);
    }

    public void RemoveTempNode(Vector2Int tempNode)
    {
        nodes.Remove(tempNode);
        
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
    public IEnumerable<Vector2Int> GetNeighbors(Vector2Int node)
    {
        return nodes.ContainsKey(node) ? nodes[node].Neighbors : null;
    }

    /// <summary> 간선 가중치 조회 </summary>
    public bool TryGetEdgeWeight(Vector2Int from, Vector2Int to, out float weight)
    {
        weight = 0;
        return nodes.ContainsKey(from) && nodes[from].EdgeWeights.TryGetValue(to, out weight);
    }

    /// <summary> 해당 방향의 모든 노드 반환 </summary>
    public IEnumerable<Vector2Int> GetNodesByDirection(Vector2Int direction)
    {
        foreach (var node in nodes.Values)
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
    public List<Vector2Int> GetNodesByDirectionOnce(Vector2Int direction)
    {
        List<Vector2Int> temp = new();
        foreach (var node in nodes.Values)
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
    
    public bool IsNodeConnected(Vector2Int node1, Vector2Int node2) => nodes[node1].Neighbors.Contains(node2) || node1 == node2;
}
