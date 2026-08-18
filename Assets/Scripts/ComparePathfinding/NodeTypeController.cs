using UnityEngine;
using System;

public class NodeTypeController
{
    private readonly NodeTypeDrawer nodeTypeDrawer = new();

    public NodeTypeDrawer NodeTypeDrawer => nodeTypeDrawer;

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
}
