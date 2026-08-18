using UnityEngine;
using System;
using System.Collections.Generic;

public class NodeList
{
    public event Action<ISelectable> OnSelected;
    public event Action<ISelectable> OnDeselected;

    private readonly NodeData nodeData;
    private readonly NodeTypeController nodeTypeController;
    private Node[,] nodes;

    public NodeTypeController NodeTypeController => nodeTypeController;
    public Node[,] Nodes => nodes;

    private int nodeSize = 1;
    public NodeList(NodeData data)
    {
        nodeData = data;
    }

    public void Initialize(int nodeSize, int mapSize)
    {
        nodeTypeController.Initialize(nodeData, nodes, GetNode);

        this.nodeSize = nodeSize;
        nodes = new Node[mapSize, mapSize];
    }

    public void SetNode(int x, int y, Node node)
    {
        node.OnSelectedCallback += OnSelected;
        node.OnDeselectedCallback += OnDeselected;
        node.Initialize(new Vector2Int(x, y));
        nodes[x, y] = node;
    }

    public Vector2Int GetNodeIndex(Vector2 position)
    {
        int x = (int)(position.x / nodeSize);
        int y = (int)(position.y / nodeSize);
        return new Vector2Int(x, y);
    }

    public Vector2 GridToWorld(Vector2Int index)
    {
        return new Vector2(index.x * nodeSize, index.y * nodeSize);
    }

    public Node GetNode(Vector2Int index) => nodes[index.x, index.y];

    private readonly List<Node> nodesInRange = new();
    public List<Node> GetNodesInRange(Vector2Int standard, float radius)
    {
        nodesInRange.Clear();

        Vector2Int nodeIndex = new();
        int range = Mathf.CeilToInt(radius / nodeSize);
        for (int x = standard.x - range; x <= standard.x + range; x++)
        {
            for (int y = standard.y - range; y <= standard.y + range; y++)
            {
                if (x < 0 || x >= nodes.GetLength(0) || y < 0 || y >= nodes.GetLength(1)) continue;
                nodeIndex.x = x; nodeIndex.y = y;

                float squareOfDistance = Vector2.SqrMagnitude(GridToWorld(standard) - GridToWorld(nodeIndex));

                if (squareOfDistance <= radius * radius)
                {
                    nodesInRange.Add(nodes[x, y]);
                }
            }
        }
        return nodesInRange;
    }

    public bool IsNodeAccessable(Vector2Int node1, Vector2Int node2)
    {
        return nodes[node1.x, node1.y].NodeArea == nodes[node2.x, node2.y].NodeArea;
    }
}
