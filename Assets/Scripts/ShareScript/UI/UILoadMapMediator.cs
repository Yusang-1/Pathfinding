using UnityEngine;
using System;

public class UILoadMapMediator : MonoBehaviour
{
    public event Func<MapData[]> OnOfficialMapListRequested;
    public event Func<MapData[]> OnPersonalMapListRequested;
    public event Action OnOpenMapListRequested;

    public event Action<MapData> OnLoadMapRequested;
    public event Action OnLoadMapFinished;
    public event Action OnLoadMapListClosedRequested;

    [SerializeField] private UILoadMap uiLoadMap;
    [SerializeField] private UILoadMapList uiLoadMapList;

    private void Start()
    {
        uiLoadMapList.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
        uiLoadMapList.OnLoadMapFinished += () => OnLoadMapFinished?.Invoke();
        uiLoadMapList.OnLoadMapListClosed += uiLoadMap.SetActiveTrue;
        uiLoadMapList.OnLoadMapListClosed += () => OnLoadMapListClosedRequested?.Invoke();
        
        uiLoadMap.OnOfficialMapListRequested += () => OnOfficialMapListRequested?.Invoke();
        uiLoadMap.OnPersonalMapListRequested += () => OnPersonalMapListRequested?.Invoke();
        uiLoadMap.OnOpenMapListRequested += () => OnOpenMapListRequested?.Invoke();

        uiLoadMap.SetProviders(uiLoadMapList.ShowMapList);        
    }

    public void ResetMediator()
    {
        gameObject.SetActive(true);
        uiLoadMap.gameObject.SetActive(true);
    }
}
