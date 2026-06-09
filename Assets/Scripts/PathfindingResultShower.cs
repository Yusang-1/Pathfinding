using UnityEngine;
using System.Collections.Generic;

public class PathfindingResultShower
{
    private Dictionary<NodeType, List<Vector2Int>> currentData;
    
    public void DrawHPAStar(NodeList nodeList, Dictionary<NodeType, List<Vector2Int>> nodeInfoDict)
    {
        currentData = nodeInfoDict;
        
        DrawSearchedTrace(nodeList, nodeInfoDict);

        if (nodeInfoDict.ContainsKey(NodeType.entrance))
        {
            SetNode(nodeList, nodeInfoDict[NodeType.entrance], NodeType.entrance);
        }
        
        DrawBasic(nodeList, nodeInfoDict);
    }
    
    public void DrawAStar(NodeList nodeList, Dictionary<NodeType, List<Vector2Int>> nodeInfoDict)
    {
        currentData = nodeInfoDict;
        
        DrawSearchedTrace(nodeList, nodeInfoDict);
        
        DrawBasic(nodeList, nodeInfoDict);
    }
    
    private void DrawSearchedTrace(NodeList nodeList, Dictionary<NodeType, List<Vector2Int>> nodeInfoDict)
    {
        if (nodeInfoDict.ContainsKey(NodeType.searched))
        {
            SetNode(nodeList, nodeInfoDict[NodeType.searched], NodeType.searched);
        }

        // if (nodeInfoDict.ContainsKey(NodeType.trace))
        // {
        //     SetNode(nodeList, nodeInfoDict[NodeType.trace], NodeType.trace);
        // }
    }
    
    private void DrawBasic(NodeList nodeList, Dictionary<NodeType, List<Vector2Int>> nodeInfoDict)
    {
        if (nodeInfoDict.ContainsKey(NodeType.obstacle))
        {
            SetNode(nodeList, nodeInfoDict[NodeType.obstacle], NodeType.obstacle);
        }
        
        if (nodeInfoDict.ContainsKey(NodeType.destination))
        {
            SetNode(nodeList, nodeInfoDict[NodeType.destination], NodeType.destination);
        }
        
        if (nodeInfoDict.ContainsKey(NodeType.destination))
        {
            SetNode(nodeList, nodeInfoDict[NodeType.unit], NodeType.unit);
        }
    }
    
    public void ResetMap(NodeList nodeList)
    {
        foreach(var item in currentData)
        {
            if(item.Key == NodeType.room) continue;
            
            SetNode(nodeList, item.Value, NodeType.room);
        }
    }

    private void SetNode(NodeList nodeList, List<Vector2Int> indexes, NodeType type)
    {
        var indexesCopy = indexes.ToArray();
        foreach (var index in indexesCopy)
        {
            nodeList.SetNodeType(index, type);
        }
    }
}
