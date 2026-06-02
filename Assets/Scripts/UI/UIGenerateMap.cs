using System;
using UnityEngine;

public class UIGenerateMap : MonoBehaviour
{
    private Action generateMap;
    public void Initialize(Action generateMap)
    {
        this.generateMap = generateMap;
    }
    
    public void OnGenerateMap()
    {
        generateMap?.Invoke();
        gameObject.SetActive(false);
    }
}
