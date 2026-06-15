using UnityEngine;
using System;
using TMPro;

public class UIMapContainer : MonoBehaviour
{
    private Action<MapData> onSelectMap;

    [SerializeField] private TextMeshProUGUI mapNameText;

    private MapData mapData;

    public void Initialize(MapData data, Action<MapData> setUIInfo)
    {
        onSelectMap ??= setUIInfo;

        mapData = data;
        mapNameText.text = data.MapName;

        gameObject.SetActive(true);
    }

    /// <summary> button에 할당 </summary>        
    public void OnSelect()
    {
        onSelectMap?.Invoke(mapData);
    }
}

