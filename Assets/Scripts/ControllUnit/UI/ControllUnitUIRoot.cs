using UnityEngine;
using System;

public class ControllUnitUIRoot : MonoBehaviour
{
    // public event Action<int, int> OnGenerateMapRequested;
    
    [SerializeField] private UILoadMap uiLoadMap;
    [SerializeField] private UIResultController uIResultController;
    
    public void Initialize()
    {
        // uiGenerateMap.OnGenerateMapRequested += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);                
    }
}
