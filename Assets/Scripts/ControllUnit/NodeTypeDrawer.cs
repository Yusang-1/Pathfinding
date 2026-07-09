using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class NodeTypeDrawer
    {
        private readonly Dictionary<NodeType, List<Vector2Int>> nodeInfoDict = new();
        private NodeList nodeList;
        private NodeData data;

        public bool IsDuringNodeSetting;

        public Vector2Int StartNodeIndex { get; private set; }
        public Vector2Int GoalNodeIndex { get; private set; }

        public void Initialize(NodeList nodeList, NodeData data)
        {
            this.nodeList = nodeList;
            this.data = data;
            IsDuringNodeSetting = true;            
        }

        public void SetNodeType(Vector2Int nodeIndex, NodeType type)
        {
            Node node = nodeList.GetNode(nodeIndex);
            NodeType currentType = node.GetNodeType();

            if (!nodeInfoDict.ContainsKey(type))
            {
                nodeInfoDict.Add(type, new List<Vector2Int>());
            }
            if (!nodeInfoDict.ContainsKey(currentType))
            {
                nodeInfoDict.Add(currentType, new List<Vector2Int>());
            }

            nodeInfoDict[currentType].Remove(nodeIndex);
            nodeInfoDict[type].Add(nodeIndex);

            if (type == NodeType.destination)
            {

                nodeInfoDict[type].Remove(GoalNodeIndex);
                nodeList.GetNode(GoalNodeIndex).SetType(NodeType.room, data.GetSprite(NodeType.room));

                GoalNodeIndex = nodeIndex;
            }

            node.SetType(type, data.GetSprite(type));
        }
    }
}
