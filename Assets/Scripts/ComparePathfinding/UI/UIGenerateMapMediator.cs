using UnityEngine;
using System;

public class UIGenerateMapMediator : MonoBehaviour
{
    /// <summary> 맵 생성의 구현 코드들 </summary>
    public event Action<int, int> OnGenerateMapRequested;
    /// <summary> 맵이 생성되었을 때 UI의 작동 </summary>
    public event Action OnGenerateMapUI;
    public event Func<MapData[]> OnOfficialMapListRequested;
    public event Func<MapData[]> OnPersonalMapListRequested;

    public event Action<MapData> OnLoadMapRequested;

    [SerializeField] private UIGenerateMap uiGenerateMap;
    [SerializeField] private UIGenerateMapInput uiGenerateMapInput;
    [SerializeField] private UILoadMapList uiLoadMapList;
    [SerializeField] private GameObject generateUIs;

    private void Start()
    {
        OnGenerateMapUI += SetActiveFalse;
        uiLoadMapList.OnLoadMapClosed += SetActiveGenerateUIs;
        uiLoadMapList.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
        uiLoadMapList.OnLoadMapEnd += () => OnGenerateMapUI?.Invoke();

        uiGenerateMap.OnGenerateMapRequested += (mapSize, clusterSize) => OnGenerateMapRequested(mapSize, clusterSize);
        uiGenerateMap.OnOfficialMapListRequested += () => OnOfficialMapListRequested?.Invoke();
        uiGenerateMap.OnPersonalMapListRequested += () => OnPersonalMapListRequested?.Invoke();
        uiGenerateMap.OnGenerateMapUI += () => OnGenerateMapUI?.Invoke();

        uiGenerateMap.SetProviders(uiGenerateMapInput.GetMapSize, uiGenerateMapInput.GetClusterSize, uiLoadMapList.ShowMapList);
    }

    public void SetActiveTrue() => gameObject.SetActive(true);
    public void SetActiveFalse() => gameObject.SetActive(false);

    private void SetActiveGenerateUIs() => generateUIs.SetActive(true);
}
