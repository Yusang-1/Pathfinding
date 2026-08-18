using UnityEngine;
using System;
using System.Collections.Generic;

public class NodeTypeController
{
    public event Action<ISelectable> OnSelected;
    public event Action<ISelectable> OnDeselected;
    private Func<Vector2Int, Node> getNodeAction;

    private readonly NodeTypeDrawer nodeTypeDrawer = new();

    private Node[,] nodes;

    public NodeTypeDrawer NodeTypeDrawer => nodeTypeDrawer;

    public void Initialize(NodeData nodeData, Node[,] nodes, Func<Vector2Int, Node> getNodeAction)
    {
        this.getNodeAction = getNodeAction;
        this.nodes = nodes;

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
