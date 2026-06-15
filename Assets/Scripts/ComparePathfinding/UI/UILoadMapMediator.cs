using UnityEngine;
using System;

public class UILoadMapMediator : MonoBehaviour
{
    /// <summary> 맵이 생성되었을 때 UI의 작동 </summary>
    private event Action OnLoadMapUI;
    public event Func<MapData[]> OnOfficialMapListRequested;
    public event Func<MapData[]> OnPersonalMapListRequested;

    public event Action<MapData> OnLoadMapRequested;

    [SerializeField] private UILoadMap uiLoadMap;
    [SerializeField] private UILoadMapList uiLoadMapList;

    private void Start()
    {
        uiLoadMapList.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
        uiLoadMapList.OnLoadMapEnd += () => OnLoadMapUI?.Invoke();

        uiLoadMap.OnOfficialMapListRequested += () => OnOfficialMapListRequested?.Invoke();
        uiLoadMap.OnPersonalMapListRequested += () => OnPersonalMapListRequested?.Invoke();
        uiLoadMap.OnLoadMapUI += () => OnLoadMapUI?.Invoke();
        uiLoadMap.SetProviders(uiLoadMapList.ShowMapList);
    }

    public void ResetMediator()
    {
        gameObject.SetActive(true);
        uiLoadMap.gameObject.SetActive(true);
    }
}
