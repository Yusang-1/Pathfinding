using UnityEngine;
using System;

public class UILoadMap : MonoBehaviour
{
    /// <summary> 맵이 생성되었을 때 UI의 작동 </summary>
    public event Action OnLoadMapUI;

    public event Func<MapData[]> OnOfficialMapListRequested;
    public event Func<MapData[]> OnPersonalMapListRequested;
    private Action<MapData[], MapData[]> ShowSavedMapsAction;


    private void Start()
    {
        OnLoadMapUI += SetActiveFalse;
    }

    public void SetProviders(Action<MapData[], MapData[]> showSavedMaps)
    {
        ShowSavedMapsAction = showSavedMaps;
    }

    /// <summary> button에 할당 </summary>
    public void OnShowSavedMapButton()
    {
        var officlaMapList = OnOfficialMapListRequested?.Invoke();
        var personalMapList = OnPersonalMapListRequested?.Invoke();
        ShowSavedMapsAction?.Invoke(officlaMapList, personalMapList);

        OnLoadMapUI?.Invoke();
    }

    public void SetActiveTrue() => gameObject.SetActive(true);
    public void SetActiveFalse() => gameObject.SetActive(false);
}
