using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class NodeInfo
    {
        private readonly Dictionary<NodeType, List<Vector2Int>> nodeInfoDict = new();
        private NodeList nodeList;
        private NodeData data;

        private bool isStartSet;
        private bool isGoalSet;
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

            if (currentType == NodeType.unit) isStartSet = false;
            else if (currentType == NodeType.destination) isGoalSet = false;

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

            if (type == NodeType.unit)
            {
                if (isStartSet) // 이미 start가 세팅되어 있을 경우 새로운 노드로 대체
                {
                    nodeInfoDict[type].Remove(StartNodeIndex);
                    nodeList.GetNode(StartNodeIndex).SetType(NodeType.room, data.GetSprite(NodeType.room));
                }
                StartNodeIndex = nodeIndex;
                isStartSet = true;
            }
            else if (type == NodeType.destination)
            {
                if (isGoalSet) // 이미 goal이 세팅되어 있을 경우 새로운 노드로 대체
                {
                    nodeInfoDict[type].Remove(GoalNodeIndex);
                    nodeList.GetNode(GoalNodeIndex).SetType(NodeType.room, data.GetSprite(NodeType.room));
                }
                GoalNodeIndex = nodeIndex;
                isGoalSet = true;
            }
            // OnPathfindAvailable(isStartSet && isGoalSet && IsDuringNodeSetting);

            node.SetType(type, data.GetSprite(type));
        }
    }
}
