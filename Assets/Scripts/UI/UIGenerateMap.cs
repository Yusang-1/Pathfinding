using UnityEngine;
using System;

public class UIGenerateMap : MonoBehaviour
{
    public event Action<int, int> OnGenerateMap;
    public event Action OnGenerateMapUI;
    
    [SerializeField] private UIGenerateMapInput input;
    
    public void GenerateMap()
    {
        input.GetInput(out int mapSize, out int clusterSize);
        OnGenerateMap?.Invoke(mapSize, clusterSize);
        OnGenerateMapUI?.Invoke();
        
        gameObject.SetActive(false);
    }
}
