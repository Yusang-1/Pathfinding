using UnityEngine;
using System;

public class Node : MonoBehaviour, ISelectable, IPoolObject<Node>
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public event Action<ISelectable, bool> OnSelectedCallback;
    public event Action<ISelectable, bool> OnDeselectedCallback;
    public virtual event Action<Node> OnPoolObjectUnused;
    public virtual event Action<Node> OnPoolObjectFirstCreated;

    public int NodeArea { get; private set; }
    public bool IsAreaSet { get => NodeArea > 0; }

    protected Vector2Int index;
    public Vector2Int Index => index;
    private NodeType type;

    public bool IsWalkable { get; private set; }

    private void Start()
    {
        OnPoolObjectFirstCreated?.Invoke(this);
    }
    
    public virtual void Initialize(Vector2Int index)
    {
        this.index = index;
        IsWalkable = true;
        type = NodeType.room;
        
        gameObject.SetActive(true);
    }

    public void Selected()
    {
        OnSelectedCallback?.Invoke(this, true);
        if (IsAreaSet) Debug.Log($"areaNum : {NodeArea}");
    }
    public void Deselected()
    {
        OnDeselectedCallback?.Invoke(this, true);
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
    public void ResetNode()
    {
        OnPoolObjectUnused?.Invoke(this);
        NodeArea = 0;
        gameObject.SetActive(false);
    }

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
