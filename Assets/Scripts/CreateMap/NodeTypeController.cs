using UnityEngine;

public class NodeTypeController
{
    private NodeType currentSelectedType;
    private NodeData nodeData;
    
    public void Initialize(NodeData nodeData)
    {
        this.nodeData = nodeData;
    }
    
    public void SetCurrentSelected(NodeType type)
    {
        currentSelectedType = type;
    }
    
    public void SetNodeType(ISelectable selectable)
    {
        if(currentSelectedType == default) currentSelectedType = NodeType.room;
        
        (selectable as Node).SetType(currentSelectedType, nodeData.GetSprite(currentSelectedType));
    }
}
