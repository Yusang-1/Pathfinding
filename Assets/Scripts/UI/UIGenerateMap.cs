using UnityEngine;
using System;

public class UIGenerateMap : MonoBehaviour
{
    public event Action<int, int> OnGenerateMap;
    
    [SerializeField] private UIGenerateMapInput input;
    
    public void GenerateMap()
    {
        input.GetInput(out int mapSize, out int clusterSize);
        OnGenerateMap?.Invoke(mapSize, clusterSize);
        
        gameObject.SetActive(false);
    }
}
