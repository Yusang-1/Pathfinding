using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Assets.Scripts.CreateMap;
using Assets.Scripts.CreateMap.UI;

namespace Assets.Scripts.ControllUnit.UI
{
    public class ControllUnitUIRoot : MonoBehaviour
    {
        // UILoadMapMediator event
        public event Action<int> OnLoadMapRequested;
        public event Func<MapData[]> OnGetOfficialMapListRequested;
        public event Func<MapData[]> OnGetPersonalMapListRequested;

        // UISpawnUnit event
        public event Action<int> OnSpawnUnitRequested;
        public event Action<Action> OnSpawnEvent;

        // UIDragController event
        public Action<Vector3> OnHoldStarted;
        public Action<Vector3> OnHoldPerformed;
        public Action OnHoldCanceled;
        public Func<Vector3, Vector3> OnECSHoldPerformed;
        public event Action<Vector3, float, float> OnFindUnitsInDragUI;

        // UIContainerScenes event
        public Action OnManageMenu;

        // UIUnitpanel event
        public Action<ISelectableUnit> OnUnitSelected;
        public Action<ISelectableUnit> OnUnitDeselected;
        public Action<string, Entity> OnECSUnitSelected;
        public Action<Entity> OnECSUnitDeselected;

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

            uiSpawnUnit.OnSpawnUnitCode += HandleOnSpawnUnit;
            uiSpawnUnit.OnSpawnEvent += HandleOnGetSpawnArea;

            OnHoldStarted += uiDragController.DragStarted;
            OnHoldPerformed += uiDragController.DragPerformed;
            OnHoldCanceled += uiDragController.DragCanceled;
            OnECSHoldPerformed += uiDragController.ECSDragPerformed;

            uiDragController.OnFindUnitsInDragUI += HandleOnFindUnitsInDragUI;

            OnManageMenu += uiContainerScenes.OnControllMenu;

            OnUnitSelected += uiUnitPanel.UnitSelected;
            OnUnitDeselected += uiUnitPanel.UnitDeselected;
            OnECSUnitSelected += uiUnitPanel.ECSUnitSelected;
            OnECSUnitDeselected += uiUnitPanel.ECSUnitDeSelected;

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

            uiSpawnUnit.OnSpawnUnitCode -= HandleOnSpawnUnit;
            uiSpawnUnit.OnSpawnEvent -= HandleOnGetSpawnArea;

            OnHoldStarted -= uiDragController.DragStarted;
            OnHoldPerformed -= uiDragController.DragPerformed;
            OnHoldCanceled -= uiDragController.DragCanceled;
            OnECSHoldPerformed -= uiDragController.ECSDragPerformed;

            uiDragController.OnFindUnitsInDragUI -= HandleOnFindUnitsInDragUI;

            OnManageMenu -= uiContainerScenes.OnControllMenu;

            OnUnitSelected -= uiUnitPanel.UnitSelected;
            OnUnitDeselected -= uiUnitPanel.UnitDeselected;
            OnECSUnitSelected -= uiUnitPanel.ECSUnitSelected;
            OnECSUnitDeselected -= uiUnitPanel.ECSUnitDeSelected;

            isBound = false;
        }

        private void HandleOnLoadMap(int mapCode)
        {
            OnLoadMapRequested?.Invoke(mapCode);
        }
        private MapData[] HandleOnGetOfficialMapList()
        {
            return OnGetOfficialMapListRequested?.Invoke();
        }
        private MapData[] HandleOnGetPersonalMapList()
        {
            return OnGetPersonalMapListRequested?.Invoke();
        }
        private void HandleOnSpawnUnit(int unitCode)
        {
            OnSpawnUnitRequested?.Invoke(unitCode);
        }
        private void HandleOnGetSpawnArea(Action action)
        {
            OnSpawnEvent?.Invoke(action);
        }
        private void HandleOnFindUnitsInDragUI(Vector3 standard, float width, float height)
        {
            OnFindUnitsInDragUI?.Invoke(standard, width, height);
        }
    }
}
