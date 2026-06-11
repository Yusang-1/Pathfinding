using UnityEngine;
using System;

public class Node : MonoBehaviour, ISelectable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public event Action<ISelectable> OnSelectedCallback;
    public event Action<ISelectable> OnDeselectedCallback;

    public int NodeArea { get; private set; }
    public bool IsAreaSet { get => NodeArea > 0; }

    private Vector2Int index;
    public Vector2Int Index => index;
    private NodeType type;

    public bool IsWalkable { get; private set; }

    public void Initialize(Vector2Int index)
    {
        this.index = index;
        IsWalkable = true;
        type = NodeType.room;
    }

    public void Selected()
    {
        OnSelectedCallback?.Invoke(this);
        if (IsAreaSet) Debug.Log($"areaNum : {NodeArea}");
    }
    public void Deselected()
    {
        OnDeselectedCallback?.Invoke(this);
    }

    public void SetType(NodeType type, Sprite sprite)
    {
        this.type = type;
        spriteRenderer.sprite = sprite;

        if (type == NodeType.obstacle)
        {
            IsWalkable = false;
        }
        else IsWalkable = true;
    }

    public void SetNodeArea(int areaNum) => NodeArea = areaNum;
    public void ResetNodeArea() => NodeArea = 0;

    public NodeType GetNodeType()
    {
        return type;
    }
}

public enum NodeType
{
    unit,
    destination,
    obstacle,
    trace,
    room,
    searched,
    entrance,
}
