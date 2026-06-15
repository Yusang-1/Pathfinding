using UnityEngine;
using System;
using System.Collections.Generic;

public class NodeInfo
{
    public event Action<bool> OnPathfindAvailable;

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
        OnPathfindAvailable(isStartSet && isGoalSet && IsDuringNodeSetting);

        node.SetType(type, data.GetSprite(type));
    }
    public void SetNodeTypeInPathFinding(Vector2Int nodeIndex, NodeType type) // 실제 sprite를 변경할 필요 없음, Dictionary에만 type을 저장
    {
        Node node = nodeList.GetNode(nodeIndex);

        if (!nodeInfoDict.ContainsKey(type))
        {
            nodeInfoDict.Add(type, new List<Vector2Int>());
        }
        nodeInfoDict[type].Add(nodeIndex);
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
                node = nodeList.GetNode(nodeIndex);
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
    public void ResetSearched()
    {
        if(!nodeInfoDict.ContainsKey(NodeType.searched)) return;
        
        Node node;
        foreach(var nodes in nodeInfoDict[NodeType.searched])
        {
            node = nodeList.GetNode(nodes);
            NodeType type = node.GetNodeType();
            if(type == NodeType.unit || type == NodeType.destination || type == NodeType.obstacle)
            {
                continue;
            }
            
            node.SetType(NodeType.room, data.GetSprite(NodeType.room));
        }
        nodeInfoDict[NodeType.searched].Clear();
    }
    public void ResetTrace()
    {
        if(!nodeInfoDict.ContainsKey(NodeType.trace)) return;
        
        Node node;
        foreach(var nodes in nodeInfoDict[NodeType.trace])
        {
            node = nodeList.GetNode(nodes);
            NodeType type = node.GetNodeType();
            if(type == NodeType.unit || type == NodeType.destination || type == NodeType.obstacle)
            {
                continue;
            }
            
            node.SetType(NodeType.room, data.GetSprite(NodeType.room));
        }
        nodeInfoDict[NodeType.trace].Clear();
    }

    public void ClearDict()
    {
        if (nodeInfoDict.ContainsKey(NodeType.trace))
            nodeInfoDict[NodeType.trace]?.Clear();
        if (nodeInfoDict.ContainsKey(NodeType.searched))
            nodeInfoDict[NodeType.searched]?.Clear();
    }

    public Dictionary<NodeType, List<Vector2Int>> GetNodeInfo() => nodeInfoDict.GetDeepCopy(value => new List<Vector2Int>(value));
}
