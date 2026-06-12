using System;
using UnityEngine;

public class UIManageMap : MonoBehaviour
{
    public event Action OnClear;
    public event Action OnRemove;
    
    /// <summary> button에 할당 </summary>    
    public void OnClearMap()
    {
        OnClear?.Invoke();
    }
    
    /// <summary> button에 할당 </summary>
    public void OnRemoveMap()
    {
        OnRemove?.Invoke();
        SetActiveFalse();
    }
    
    public void SetActiveTrue() => gameObject.SetActive(true);
    public void SetActiveFalse() => gameObject.SetActive(false);
}
