using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit.UI
{
    public class ControllUnitUIRoot : MonoBehaviour
    {
        // UILoadMapMediator event
        public event Action<MapData> OnLoadMapRequested;
        public event Func<MapData[]> OnGetOfficialMapListRequested;
        public event Func<MapData[]> OnGetPersonalMapListRequested;

        // UISpawnUnit event
        public event Action OnSpawnUnitRequested;

        [SerializeField] private UILoadMapMediator uiLoadMapMediator;
        [SerializeField] private UIResultController uiResultController;
        [SerializeField] private UISpawnUnit uiSpawnUnit;
        [SerializeField] private UIUnitpanel uiUnitPanel;

        private void Start()
        {
            uiLoadMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
            uiLoadMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
            uiLoadMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();
            uiLoadMapMediator.OnLoadMapEnd += uiSpawnUnit.SetActiveTrue;
            uiLoadMapMediator.OnLoadMapEnd += uiUnitPanel.SetActiveTrue;
            
            uiSpawnUnit.OnSpawnUnitRequested += () => OnSpawnUnitRequested?.Invoke();
        }
        
        public void UnitSelected(ISelectableUnit unit)
        {
            uiUnitPanel.UnitSelected(unit);
        }
        public void UnitDeselected(ISelectableUnit unit)
        {
            uiUnitPanel.UnitDeselected(unit);
        }
    }
}
