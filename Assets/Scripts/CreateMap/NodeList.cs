using UnityEngine;
using System;

namespace Assets.Scripts.CreateMap
{
    public class NodeList
    {
        public event Action<ISelectable> OnSelected;
        public event Action<ISelectable> OnDeselected;

        private readonly NodeInfo nodeInfo;
        private Node[,] nodes;
        
        public NodeInfo NodeInfo => nodeInfo;
        
        public NodeList(NodeData data)
        {
            nodeInfo = new NodeInfo(this, data);
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

        public Node GetNode(Vector2Int index) => nodes[index.x, index.y];
    }
}
