using UnityEngine;
using System;

public class UIExportMap : MonoBehaviour
{
    public event Action<string> OnExprotMap;
    
    [SerializeField] private UIExportMapInput input;
    
    /// <summary> button에 할당 </summary>
    public void OnExportMap()
    {
        string mapName = input.GetValue();
        OnExprotMap?.Invoke(mapName);
    }
    
    public void SetActiveTrue() => gameObject.SetActive(true);
}
