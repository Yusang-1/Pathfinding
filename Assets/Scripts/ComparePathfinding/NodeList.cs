using UnityEngine;
using System;
using System.Collections.Generic;

public class NodeList
{
    public event Action<ISelectable> OnSelected;
    public event Action<ISelectable> OnDeselected;

    private readonly NodeData nodeData;
    private readonly NodeInfo nodeInfo = new();
    private Node[,] nodes;

    public NodeInfo NodeInfo => nodeInfo;
    public Node[,] Nodes => nodes;

    private int nodeSize = 1;
    public NodeList(NodeData data)
    {
        nodeData = data;
    }

    public void Initialize(int nodeSize, int mapSize)
    {
        nodeInfo.Initialize(this, nodeData);

        this.nodeSize = nodeSize;
        nodes = new Node[mapSize, mapSize];
    }

    public void ResetTrace()
    {
        nodeInfo.ResetTraces();
    }
    public void ResetAll()
    {
        nodeInfo.ResetAllNode();
        ResetAllNode();
    }

    public Vector2Int GetNodeIndex(Vector2 position)
    {
        int x = (int)(position.x / nodeSize);
        int y = (int)(position.y / nodeSize);
        return new Vector2Int(x, y);
    }

    public void CreateNodeArray(int mapSize)
    {
        nodes = new Node[mapSize, mapSize];
    }

    public void SetNode(int x, int y, Node node)
    {
        node.OnSelectedCallback += OnSelected;
        node.OnDeselectedCallback += OnDeselected;
        node.Initialize(new Vector2Int(x, y));
        nodes[x, y] = node;
    }

    public void SetNodeType(Vector2Int index, NodeType type)
    {
        nodeInfo.SetNodeType(index, type);
    }
    public void SetNodeTypeInPathFinding(Vector2Int index, NodeType type)
    {
        nodeInfo.SetNodeTypeInPathFinding(index, type);
    }

    public Vector2 GridToWorld(Vector2Int index)
    {
        return new Vector2(index.x * nodeSize, index.y * nodeSize);
    }

    public Node GetNode(Vector2Int index) => nodes[index.x, index.y];

    private int currentAreaNum;
    private readonly Dictionary<int, List<Vector2Int>> nodesByAreaNum = new();
    public void SetNodeArea()
    {
        currentAreaNum = 1;
        int xLength = nodes.GetLength(0);
        int yLength = nodes.GetLength(1);

        int leftNodeArea, downNodeArea;

        Vector2Int curNode, leftNode, downNode;
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < yLength; j++)
            {
                if (!nodes[i, j].IsWalkable) continue;
                curNode = new Vector2Int(i, j);

                leftNode = curNode + Vector2Int.left;
                if (!(leftNode.x < 0 || leftNode.y < 0 || leftNode.x >= xLength || leftNode.y >= yLength))
                {
                    var node = GetNode(leftNode);
                    if (node.IsAreaSet)
                        leftNodeArea = node.NodeArea;
                    else
                        leftNodeArea = -1;
                }
                else leftNodeArea = -1;

                downNode = curNode + Vector2Int.down;
                if (!(downNode.x < 0 || downNode.y < 0 || downNode.x >= xLength || downNode.y >= yLength))
                {
                    var node = GetNode(downNode);
                    if (node.IsAreaSet)
                        downNodeArea = node.NodeArea;
                    else
                        downNodeArea = -1;
                }
                else downNodeArea = -1;

                // 값 적용
                if (leftNodeArea > 0 && downNodeArea > 0) // 둘다 값이 있을 때
                {
                    if (leftNodeArea == downNodeArea)
                    {
                        // 값이 같은 경우 curNode도 해당 값으로 적용
                        SetNodeAreaToNode(i, j, leftNodeArea);
                    }
                    else
                    {
                        // 둘중 값을 가진 노드가 더 많은 쪽으로 흡수
                        int bigAreaNum = CompareCountOfNodeArea(leftNodeArea, downNodeArea);
                        SetNodeAreaToNode(i, j, bigAreaNum);
                    }
                }
                else if (leftNodeArea < 0 && downNodeArea < 0) // 둘다 값이 없을 때
                {
                    // currentAreaNum + 1을 curNode에 적용
                    currentAreaNum++;
                    SetNodeAreaToNode(i, j, currentAreaNum);
                }
                else // 둘중 하나는 값이 있을 때
                {
                    // 값이 있는 쪽을 적용
                    int value = leftNodeArea > 0 ? leftNodeArea : downNodeArea;
                    SetNodeAreaToNode(i, j, value);
                }
            }
        }
    }
    private void ResetAllNode()
    {
        foreach (var value in nodesByAreaNum.Values)
        {
            value.Clear();
        }
        for (int i = 0; i < nodes.GetLength(0); i++)
        {
            for (int j = 0; j < nodes.GetLength(1); j++)
            {
                nodes[i, j].ResetNode();
                nodes[i, j].OnSelectedCallback -= OnSelected;
                nodes[i, j].OnDeselectedCallback -= OnDeselected;
            }
        }
    }
    public bool IsNodeAccessable(Vector2Int node1, Vector2Int node2)
    {
        return nodes[node1.x, node1.y].NodeArea == nodes[node2.x, node2.y].NodeArea;
    }

    private void SetNodeAreaToNode(int x, int y, int value)
    {
        if (!nodesByAreaNum.ContainsKey(value))
        {
            nodesByAreaNum.Add(value, new List<Vector2Int>());
        }
        nodes[x, y].SetNodeArea(value);
        nodesByAreaNum[value].Add(new Vector2Int(x, y));
    }

    private int CompareCountOfNodeArea(int num1, int num2)
    {
        int num1Count = nodesByAreaNum[num1].Count;
        int num2Count = nodesByAreaNum[num2].Count;

        int smaller = num1Count < num2Count ? num1 : num2;
        int bigAreaNum = num1Count < num2Count ? num2 : num1;

        List<Vector2Int> list = nodesByAreaNum[smaller];
        foreach (var index in list)
        {
            nodes[index.x, index.y].SetNodeArea(bigAreaNum);
        }

        return bigAreaNum;
    }
}
