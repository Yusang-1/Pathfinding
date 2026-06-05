using UnityEngine;
using System;
using System.Collections.Generic;

public class NodeInfo
{
    public event Action<bool> OnPathfindAvailable;

    private readonly Dictionary<NodeType, List<Vector2Int>> nodeInfoDict = new();
    private readonly NodeList nodeList;
    private readonly NodeData data;

    private bool isStartSet;
    private bool isGoalSet;
    public bool IsDuringNodeSetting;

    public Vector2Int StartNodeIndex { get; private set; }
    public Vector2Int GoalNodeIndex { get; private set; }

    public NodeInfo(NodeList nodeList, NodeData data)
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
        OnPathfindAvailable(isStartSet && isGoalSet && IsDuringNodeSetting);

        node.SetType(type, data.GetSprite(type));
    }
    public void SetNodeTypeInPathFinding(Vector2Int nodeIndex, NodeType type)
    {
        Node node = nodeList.GetNode(nodeIndex);
        NodeType currentType = node.GetNodeType();
        if (currentType == NodeType.unit || currentType == NodeType.destination || currentType == NodeType.obstacle) return;
        if (currentType == NodeType.trace && type == NodeType.searched) return;

        if (!nodeInfoDict.ContainsKey(type))
        {
            nodeInfoDict.Add(type, new List<Vector2Int>());
        }

        nodeInfoDict[type].Add(nodeIndex);

        if (type == NodeType.obstacle)
        {
            node.SetType(type, data.GetSprite(type));
        }
        else if (type == NodeType.unit)
        {
            isStartSet = true;
            StartNodeIndex = nodeIndex;
            node.SetType(type, data.GetSprite(type));
        }
        else if (type == NodeType.destination)
        {
            isGoalSet = true;
            GoalNodeIndex = nodeIndex;
            node.SetType(type, data.GetSprite(type));
        }
    }

    public void ShowAStarPath()
    {
        ShowNodeColor(NodeType.searched);
        ShowNodeColor(NodeType.trace);

        ShowBasicNodes();
    }

    public void ShowHPAStarPath()
    {
        ShowNodeColor(NodeType.searched);
        ShowNodeColor(NodeType.trace);
        ShowNodeColor(NodeType.entrance);
        ShowNodeColor(NodeType.entranceUsed);

        ShowBasicNodes();
    }

    private void ShowNodeColor(NodeType type)
    {
        if (nodeInfoDict.ContainsKey(type))
        {
            List<Vector2Int> nodes = nodeInfoDict[type];
            foreach (var node in nodes)
            {
                nodeList.Nodes[node.x, node.y].SetType(type, data.GetSprite(type));
            }
        }
    }

    private void ShowBasicNodes()
    {
        ShowNodeColor(NodeType.unit);
        ShowNodeColor(NodeType.destination);
        ShowNodeColor(NodeType.obstacle);
    }

    public void ResetAllNodes()
    {
        Vector2Int[] nodesCopy;
        foreach (var nodes in nodeInfoDict.Values)
        {
            nodesCopy = nodes.ToArray();
            foreach (var nodeIndex in nodesCopy)
            {          
                SetNodeType(nodeIndex, NodeType.room);
            }
        }
        foreach (var list in nodeInfoDict.Values)
        {
            list.Clear();
        }
    }

    public void ResetTraces()
    {
        Node node;
        NodeType type;
        foreach (var nodes in nodeInfoDict.Values)
        {
            foreach (var nodeIndex in nodes)
            {
                node = nodeList.Nodes[nodeIndex.x, nodeIndex.y];
                type = node.GetNodeType();

                if (type == NodeType.unit || type == NodeType.destination || type == NodeType.obstacle) continue;
                node.SetType(NodeType.room, data.GetSprite(NodeType.room));
            }
        }
        foreach (var item in nodeInfoDict)
        {
            if (item.Key == NodeType.unit || item.Key == NodeType.destination || item.Key == NodeType.obstacle) continue;

            item.Value.Clear();
        }
    }

    public void ClearDict()
    {
        if (nodeInfoDict.ContainsKey(NodeType.entranceUsed))
            nodeInfoDict[NodeType.entranceUsed]?.Clear();
        if (nodeInfoDict.ContainsKey(NodeType.trace))
            nodeInfoDict[NodeType.trace]?.Clear();
        if (nodeInfoDict.ContainsKey(NodeType.searched))
            nodeInfoDict[NodeType.searched]?.Clear();
    }

    public Dictionary<NodeType, List<Vector2Int>> GetNodeInfo() => nodeInfoDict.GetDeepCopy<NodeType, List<Vector2Int>>();
}
