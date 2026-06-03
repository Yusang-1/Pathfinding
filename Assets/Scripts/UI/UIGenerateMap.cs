using System;
using UnityEngine;

public class UIGenerateMap : MonoBehaviour
{
    private Action<int, int> generateMap;
    
    [SerializeField] private UIGenerateMapInput input;
    
    public void Initialize(Action<int, int> generateMap)
    {
        this.generateMap = generateMap;
    }
    
    public void OnGenerateMap()
    {
        input.GetInput(out int mapSize, out int clusterSize);
        generateMap?.Invoke(mapSize, clusterSize);
        gameObject.SetActive(false);
    }
}
