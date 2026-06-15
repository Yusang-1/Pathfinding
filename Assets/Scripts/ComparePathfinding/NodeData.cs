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
        spriteGetter.Add(NodeType.unit, spriteUnit);
        spriteGetter.Add(NodeType.destination, spriteDestination);
        spriteGetter.Add(NodeType.obstacle, spriteObstacle);
        spriteGetter.Add(NodeType.room, spriteRoom);
        spriteGetter.Add(NodeType.searched, spriteSearched);
        spriteGetter.Add(NodeType.entrance, spriteEntrance);
    }

    public Sprite GetSprite(NodeType type)
    {
        return spriteGetter[type];
    }
}
