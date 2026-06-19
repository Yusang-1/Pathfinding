using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NodeData", menuName = "Scriptable Objects/NodeData")]
public class NodeData : ScriptableObject
{
    [SerializeField] private Sprite spriteUnit;
    [SerializeField] private Sprite spriteDestination;
    [SerializeField] private Sprite spriteObstacle;
    [SerializeField] private Sprite spriteRoom;
    [SerializeField] private Sprite spriteSearched;
    [SerializeField] private Sprite spriteEntrance;

    private readonly Dictionary<NodeType, Sprite> spriteGetter = new();

    public void Initialize()
    {
        if(!spriteGetter.ContainsKey(NodeType.unit))
        {
            spriteGetter.Add(NodeType.unit, spriteUnit);
        }
        if(!spriteGetter.ContainsKey(NodeType.destination))
        {
            spriteGetter.Add(NodeType.destination, spriteDestination);
        }
        if(!spriteGetter.ContainsKey(NodeType.obstacle))
        {
            spriteGetter.Add(NodeType.obstacle, spriteObstacle);
        }
        if(!spriteGetter.ContainsKey(NodeType.room))
        {
            spriteGetter.Add(NodeType.room, spriteRoom);
        }
        if(!spriteGetter.ContainsKey(NodeType.searched))
        {
            spriteGetter.Add(NodeType.searched, spriteSearched);
        }
        if(!spriteGetter.ContainsKey(NodeType.entrance))
        {
            spriteGetter.Add(NodeType.entrance, spriteEntrance);
        }
    }

    public Sprite GetSprite(NodeType type)
    {
        return spriteGetter[type];
    }
}
