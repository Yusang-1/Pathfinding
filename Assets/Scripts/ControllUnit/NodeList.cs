using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class NodeList
    {
        public event Action<ISelectable> OnSelected;
        public event Action<ISelectable> OnDeselected;

        private readonly NodeTypeDrawer nodeTypeDrawer = new();
        private Node[,] nodes;

        private int nodeSize = 1;

        public Node[,] Nodes => nodes;
        public int NodeSize => nodeSize;

        public void Initialize(in MapData mapData, NodeData data)
        {
            nodeTypeDrawer.Initialize(this, data);

            nodeSize = mapData.NodeSize;            
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
            nodeTypeDrawer.SetNodeType(index, type);
        }

        /// <summary> 실제 position을 받아 Node Index를 반환   </summary>
        public Vector2Int GetNodeIndex(Vector2 position)
        {
            int x = (int)(position.x / nodeSize);
            int y = (int)(position.y / nodeSize);
            return new Vector2Int(x, y);
        }

        /// <summary> Node의 실제 월드 좌표를 반환 </summary>
        public Vector2 GridToWorld(Vector2Int index) => new(index.x * nodeSize, index.y * nodeSize);

        public Node GetNode(Vector2Int index) => nodes[index.x, index.y];

        public bool IsNodeAccessable(Vector2Int node1, Vector2Int node2) => nodes[node1.x, node1.y].NodeArea == nodes[node2.x, node2.y].NodeArea;

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
        
        public bool IsNodesInRangeWalkable(Vector2Int standardNode, float unitRadius)
        {
            if(GetNode(standardNode).IsWalkable == false) return false;
            
            var nodes = GetNodesInRange(standardNode, unitRadius);
            
            for(int i = 0; i < nodes.Count; i++)
            {
                if(nodes[i].IsWalkable == false)
                {
                    return false;
                }
            }
            
            return true;
        }

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
}
