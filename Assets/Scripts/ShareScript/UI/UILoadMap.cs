using UnityEngine;
using System;

public class UILoadMap : MonoBehaviour
{
    public event Func<MapData[]> OnOfficialMapListRequested;
    public event Func<MapData[]> OnPersonalMapListRequested;
    public event Action OnOpenMapListRequested;
    private Action<MapData[], MapData[]> ShowSavedMapsAction;

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
        OnOpenMapListRequested?.Invoke();

        SetActiveFalse();
    }

    public void SetActiveTrue() => gameObject.SetActive(true);
    public void SetActiveFalse() => gameObject.SetActive(false);
}
