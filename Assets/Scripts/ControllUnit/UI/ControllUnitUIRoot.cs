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
        public event Action<UnitSize> OnSpawnUnitRequested;
        public event Action<Action> OnGetSpawnAreaRequested;

        // UIDragController event
        public Action<Vector3> OnHoldStarted;
        public Action<Vector3> OnHoldPerformed;
        public Action OnHoldCanceled;
        public event Func<Vector3, float, float, HashSet<ISelectableUnit>> OnFindSelectableUnitInDragUI;
        public event Action<HashSet<ISelectableUnit>> OnUnitFocused;

        // UIContainerScenes event
        public Action OnManageMenu;
        
        // UIUnitpanel event
        public Action<ISelectableUnit> OnUnitSelected;
        public Action<ISelectableUnit> OnUnitDeselected;

        [SerializeField] private UILoadMapMediator uiLoadMapMediator;
        [SerializeField] private UIResultController uiResultController;
        [SerializeField] private UISpawnUnit uiSpawnUnit;
        [SerializeField] private UIUnitpanel uiUnitPanel;
        [SerializeField] private UIDragController uiDragController;
        [SerializeField] private UIContainerScenes uiContainerScenes;

        private void Start()
        {
            uiLoadMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
            uiLoadMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
            uiLoadMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();
            uiLoadMapMediator.OnLoadMapFinished += uiSpawnUnit.SetActiveTrue;
            uiLoadMapMediator.OnLoadMapFinished += uiUnitPanel.SetActiveTrue;

            uiSpawnUnit.OnSpawnUnitRequested += (unitSize) => OnSpawnUnitRequested?.Invoke(unitSize);
            uiSpawnUnit.OnGetSpawnAreaRequested += (action) => OnGetSpawnAreaRequested?.Invoke(action);

            OnHoldStarted += uiDragController.DragStarted;
            OnHoldPerformed += uiDragController.DragPerformed;
            OnHoldCanceled += uiDragController.DragCanceled;

            uiDragController.OnFindSelectableUnitInDragUI += (standard, x, y) => OnFindSelectableUnitInDragUI?.Invoke(standard, x, y);
            uiDragController.OnUnitFocused += (units) => OnUnitFocused?.Invoke(units);
            
            OnManageMenu += uiContainerScenes.OnControllMenu;
            
            OnUnitSelected += uiUnitPanel.UnitSelected;
            OnUnitDeselected += uiUnitPanel.UnitDeselected;
        }
    }
}
