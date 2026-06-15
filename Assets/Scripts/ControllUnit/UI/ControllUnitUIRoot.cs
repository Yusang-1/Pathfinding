using UnityEngine;
using System;

public class ControllUnitUIRoot : MonoBehaviour
{
    public event Action<MapData> OnLoadMapRequested;
    public event Func<MapData[]> OnGetOfficialMapListRequested;
    public event Func<MapData[]> OnGetPersonalMapListRequested;
    
    [SerializeField] private UILoadMapMediator uILoadMapMediator;
    [SerializeField] private UIResultController uIResultController;
    
    private void Start()
    {
        uILoadMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
        uILoadMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
        uILoadMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();
    }
}
