using System;
using UnityEngine;

public class UITileSelector : MonoBehaviour
{
    public event Action<NodeType> OnTileSelect;

    /// <summary> button에 할당 </summary>
    public void OnSelectObstacle()
    {
        OnTileSelect?.Invoke(NodeType.obstacle);
    }

    /// <summary> button에 할당 </summary>
    public void OnSelectRoom()
    {
        OnTileSelect?.Invoke(NodeType.room);
    }
    
    public void SetActiveTrue() => gameObject.SetActive(true);
}
