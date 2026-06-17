using UnityEngine;
using System;
using System.Collections.Generic;

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
        
        // UIDragController event
        public Action<Vector3> OnHoldStarted;
        public Action<Vector3> OnHoldPreformed;
        public Func<List<ISelectableUnit>> OnHoldCanceled;

        [SerializeField] private UILoadMapMediator uiLoadMapMediator;
        [SerializeField] private UIResultController uiResultController;
        [SerializeField] private UISpawnUnit uiSpawnUnit;
        [SerializeField] private UIUnitpanel uiUnitPanel;
        [SerializeField] private UIDragController uiDragController;

        private void Start()
        {
            uiLoadMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
            uiLoadMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
            uiLoadMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();
            uiLoadMapMediator.OnLoadMapFinished += uiSpawnUnit.SetActiveTrue;
            uiLoadMapMediator.OnLoadMapFinished += uiUnitPanel.SetActiveTrue;
            
            uiSpawnUnit.OnSpawnUnitRequested += () => OnSpawnUnitRequested?.Invoke();
            
            OnHoldStarted += uiDragController.DragStarted;
            OnHoldPreformed += uiDragController.DragPerformed;
            OnHoldCanceled += uiDragController.DragCanceled;
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
