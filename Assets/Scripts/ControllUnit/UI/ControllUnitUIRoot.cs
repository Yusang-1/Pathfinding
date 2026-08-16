using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.ControllUnit.SO;

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

        private bool isBound;

        private void OnEnable()
        {
            BindEvents();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void BindEvents()
        {
            if (isBound) return; // 중복 이벤트 구독 방지

            uiLoadMapMediator.OnLoadMapRequested += HandleOnLoadMap;
            uiLoadMapMediator.OnOfficialMapListRequested += HandleOnGetOfficialMapList;
            uiLoadMapMediator.OnPersonalMapListRequested += HandleOnGetPersonalMapList;
            uiLoadMapMediator.OnLoadMapFinished += uiSpawnUnit.SetActiveTrue;
            uiLoadMapMediator.OnLoadMapFinished += uiUnitPanel.SetActiveTrue;

            uiSpawnUnit.OnSpawnUnitRequested += HandleOnSpawnUnit;
            uiSpawnUnit.OnGetSpawnAreaRequested += HandleOnGetSpawnArea;

            OnHoldStarted += uiDragController.DragStarted;
            OnHoldPerformed += uiDragController.DragPerformed;
            OnHoldCanceled += uiDragController.DragCanceled;

            uiDragController.OnFindSelectableUnitInDragUI += HandleOnFindSelectableUnitInDragUI;
            uiDragController.OnUnitFocused += HandleOnUnitFocused;

            OnManageMenu += uiContainerScenes.OnControllMenu;

            OnUnitSelected += uiUnitPanel.UnitSelected;
            OnUnitDeselected += uiUnitPanel.UnitDeselected;

            isBound = true;
        }

        private void UnbindEvents()
        {
            if (!isBound) return;

            uiLoadMapMediator.OnLoadMapRequested -= HandleOnLoadMap;
            uiLoadMapMediator.OnOfficialMapListRequested -= HandleOnGetOfficialMapList;
            uiLoadMapMediator.OnPersonalMapListRequested -= HandleOnGetPersonalMapList;
            uiLoadMapMediator.OnLoadMapFinished -= uiSpawnUnit.SetActiveTrue;
            uiLoadMapMediator.OnLoadMapFinished -= uiUnitPanel.SetActiveTrue;

            uiSpawnUnit.OnSpawnUnitRequested -= HandleOnSpawnUnit;
            uiSpawnUnit.OnGetSpawnAreaRequested -= HandleOnGetSpawnArea;

            OnHoldStarted -= uiDragController.DragStarted;
            OnHoldPerformed -= uiDragController.DragPerformed;
            OnHoldCanceled -= uiDragController.DragCanceled;

            uiDragController.OnFindSelectableUnitInDragUI -= HandleOnFindSelectableUnitInDragUI;
            uiDragController.OnUnitFocused -= HandleOnUnitFocused;

            OnManageMenu -= uiContainerScenes.OnControllMenu;

            OnUnitSelected -= uiUnitPanel.UnitSelected;
            OnUnitDeselected -= uiUnitPanel.UnitDeselected;

            isBound = false;
        }

        private void HandleOnLoadMap(MapData mapData)
        {
            OnLoadMapRequested?.Invoke(mapData);
        }
        private MapData[] HandleOnGetOfficialMapList()
        {
            return OnGetOfficialMapListRequested?.Invoke();
        }
        private MapData[] HandleOnGetPersonalMapList()
        {
            return OnGetPersonalMapListRequested?.Invoke();
        }
        private void HandleOnSpawnUnit(UnitSize size)
        {
            OnSpawnUnitRequested?.Invoke(size);
        }
        private void HandleOnGetSpawnArea(Action action)
        {
            OnGetSpawnAreaRequested?.Invoke(action);
        }
        private HashSet<ISelectableUnit> HandleOnFindSelectableUnitInDragUI(Vector3 standard, float x, float y)
        {
            return OnFindSelectableUnitInDragUI?.Invoke(standard, x, y);
        }
        private void HandleOnUnitFocused(HashSet<ISelectableUnit> units)
        {
            OnUnitFocused?.Invoke(units);
        }
    }
}
