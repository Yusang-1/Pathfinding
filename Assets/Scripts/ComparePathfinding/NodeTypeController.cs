using UnityEngine;
using System;

public class NodeTypeController
{
    private readonly NodeTypeDrawer nodeTypeDrawer = new();

    public NodeTypeDrawer NodeTypeDrawer => nodeTypeDrawer;

    private NodeType currentSelectedType;

    public void Initialize(NodeData nodeData, Func<Vector2Int, Node> getNodeAction)
    {
        nodeTypeDrawer.Initialize(nodeData, getNodeAction);
    }

    public void ResetTrace()
    {
        nodeTypeDrawer.ResetTraces();
    }

    public void SetNodeType(Vector2Int index, NodeType type)
    {
        nodeTypeDrawer.SetNodeType(index, type);
    }

    public void SetNodeTypeInPathFinding(Vector2Int index, NodeType type)
    {
        nodeTypeDrawer.SetNodeTypeInPathFinding(index, type);
    }

    public void SetCurrentSelected(NodeType type)
    {
        currentSelectedType = type;
    }

    public void SetNodeType(ISelectable selectable, bool value)
    {
        if (currentSelectedType == default) currentSelectedType = NodeType.obstacle;
        SetNodeType((selectable as Node).Index, currentSelectedType);
    }
}
