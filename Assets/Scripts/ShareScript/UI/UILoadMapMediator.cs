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

    private void OnEnable()
    {
        BindEvents();
    }
    
    private void Start()
    {
        uiLoadMap.SetProviders(uiLoadMapList.ShowMapList);
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void BindEvents()
    {
        uiLoadMapList.OnLoadMapRequested += HandleLoadMap;
        uiLoadMapList.OnLoadMapFinished += HandleLoadMapFinished;
        uiLoadMapList.OnLoadMapFinished += SetActiveFalse;
        uiLoadMapList.OnLoadMapListClosed += uiLoadMap.SetActiveTrue;
        uiLoadMapList.OnLoadMapListClosed += HandleLoadMapListClosed;

        uiLoadMap.OnOfficialMapListRequested += HandleGetOfficialMapList;
        uiLoadMap.OnPersonalMapListRequested += HandleGetPersonalMapList;
        uiLoadMap.OnOpenMapListRequested += HandleOpenMapList;
    }

    private void UnbindEvents()
    {
        uiLoadMapList.OnLoadMapRequested -= HandleLoadMap;
        uiLoadMapList.OnLoadMapFinished -= HandleLoadMapFinished;
        uiLoadMapList.OnLoadMapFinished -= SetActiveFalse;
        uiLoadMapList.OnLoadMapListClosed -= uiLoadMap.SetActiveTrue;
        uiLoadMapList.OnLoadMapListClosed -= HandleLoadMapListClosed;

        uiLoadMap.OnOfficialMapListRequested -= HandleGetOfficialMapList;
        uiLoadMap.OnPersonalMapListRequested -= HandleGetPersonalMapList;
        uiLoadMap.OnOpenMapListRequested -= HandleOpenMapList;
    }

    private void HandleLoadMap(MapData mapData)
    {
        OnLoadMapRequested?.Invoke(mapData);
    }
    private void HandleLoadMapFinished()
    {
        OnLoadMapFinished?.Invoke();
    }
    private void HandleLoadMapListClosed()
    {
        OnLoadMapListClosedRequested?.Invoke();
    }
    private MapData[] HandleGetOfficialMapList()
    {
        return OnOfficialMapListRequested?.Invoke();
    }
    private MapData[] HandleGetPersonalMapList()
    {
        return OnPersonalMapListRequested?.Invoke();
    }
    private void HandleOpenMapList()
    {
        OnOpenMapListRequested?.Invoke();
    }


    public void ResetMediator()
    {
        gameObject.SetActive(true);
        uiLoadMap.gameObject.SetActive(true);
    }

    private void SetActiveFalse() => gameObject.SetActive(false);
}
