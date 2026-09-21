using UnityEngine;
using System;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.CreateMap;

namespace Assets.Scripts.ControllUnit
{
    public class MapBootStrapper
    {
        private readonly Action<int> initializeMapRuntime;

        private readonly SelectableController selectableController = new();
        private readonly MapdataJsonConverter mapdataJsonConverter;

        private readonly ControllUnitUIRoot uiRoot;
        private readonly InputManager inputManager;
        private readonly UnitSpawner unitSpawner;
        private readonly MapRuntimeContext mapRuntimeContext;
        private readonly UnitSpawnHolder unitSpawnHolder;

        private bool isBound;

        public MapBootStrapper(ControllUnitUIRoot uiRoot, InputManager inputManager, UnitSpawner unitSpawner, Action<int> initializeMapRuntime, MapRuntimeContext mapRuntimeContext)
        {
            this.uiRoot = uiRoot;
            this.inputManager = inputManager;
            this.unitSpawner = unitSpawner;
            this.initializeMapRuntime = initializeMapRuntime;
            this.mapRuntimeContext = mapRuntimeContext;
            mapdataJsonConverter = new(mapRuntimeContext.LoadedMapData);
            unitSpawnHolder = new(unitSpawner);
        }

        public void Initialize(NodeData nodeData, UnitsSO unitsSO, PathfinderControllUnit pathfinder)
        {
            nodeData.Initialize();
            unitSpawner.Initialize(new UnitRuntimeContext(pathfinder, mapRuntimeContext.SpatialHash));
            inputManager.Initialize(selectableController);

            unitsSO.Initialize();
        }

        public void BindEvents()
        {
            if (isBound) return; // 중복 이벤트 구독 방지

            AddUIRootEvent(initializeMapRuntime, mapRuntimeContext);
            AddUnitSpawnerEvent();
            AddInputManagerEvent();

            isBound = true;
        }

        public void UnbindEvents()
        {
            if (!isBound) return;

            RemoveUIRootEvent(initializeMapRuntime, mapRuntimeContext);
            RemoveUnitSpawnerEvent();
            RemoveInputManagerEvent();

            isBound = false;
        }

        private void AddUIRootEvent(Action<int> SetMapData, MapRuntimeContext mapRuntimeContext)
        {
            uiRoot.OnLoadMapRequested += SetMapData;
            uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;

            uiRoot.OnSpawnUnitRequested += unitSpawnHolder.ReserveSpawnUnitCode;

            uiRoot.OnSpawnEvent += unitSpawner.StartSetSpawnArea;
            uiRoot.OnFindSelectableUnitInDragUI += mapRuntimeContext.SpatialHash.GetUnitsInRange;
            uiRoot.OnUnitFocused += selectableController.UnitFocusedList;
        }

        private void AddUnitSpawnerEvent()
        {
            unitSpawner.OnUnitSelected += HandleUnitSelected;
            unitSpawner.OnUnitDeselected += HandleUnitDeselected;
            unitSpawner.OnSpawnAreaSettingStarted += inputManager.ChangeActionMapSelected;
        }

        private void AddInputManagerEvent()
        {
            inputManager.OnHoldStarted += HandleHoldStart;
            inputManager.OnHoldPerformed += HandleHoldPerformed;
            inputManager.OnHoldCanceled += HandleHoldCanceled;
            inputManager.OnControllMenu += HandleManageMenu;
            inputManager.OnSpawnUnitRequested += unitSpawnHolder.Spawn;
            inputManager.OnSetSpawnAreaFinished += unitSpawner.FinishSetSpawnArea;
            inputManager.OnTrackMouse += unitSpawnHolder.MovePreviewUnit;
            inputManager.OnCancelSpawnAreaSet += unitSpawnHolder.CancelSpawn;
            inputManager.OnPointerNotOverGameObject += unitSpawnHolder.HidePreviewUnit;
        }

        private void RemoveUIRootEvent(Action<int> SetMapData, MapRuntimeContext mapRuntimeContext)
        {
            uiRoot.OnLoadMapRequested -= SetMapData;
            uiRoot.OnGetOfficialMapListRequested -= mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested -= mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnSpawnUnitRequested -= unitSpawnHolder.ReserveSpawnUnitCode;
            uiRoot.OnSpawnEvent -= unitSpawner.StartSetSpawnArea;
            uiRoot.OnFindSelectableUnitInDragUI -= mapRuntimeContext.SpatialHash.GetUnitsInRange;
            uiRoot.OnUnitFocused -= selectableController.UnitFocusedList;
        }

        private void RemoveUnitSpawnerEvent()
        {
            unitSpawner.OnUnitSelected -= HandleUnitSelected;
            unitSpawner.OnUnitDeselected -= HandleUnitDeselected;
            unitSpawner.OnSpawnAreaSettingStarted -= inputManager.ChangeActionMapSelected;
        }

        private void RemoveInputManagerEvent()
        {
            inputManager.OnHoldStarted -= HandleHoldStart;
            inputManager.OnHoldPerformed -= HandleHoldPerformed;
            inputManager.OnHoldCanceled -= HandleHoldCanceled;
            inputManager.OnControllMenu -= HandleManageMenu;
            inputManager.OnSpawnUnitRequested -= unitSpawnHolder.Spawn;
            inputManager.OnSetSpawnAreaFinished -= unitSpawner.FinishSetSpawnArea;
            inputManager.OnTrackMouse -= unitSpawnHolder.MovePreviewUnit;
            inputManager.OnCancelSpawnAreaSet -= unitSpawnHolder.CancelSpawn;
            inputManager.OnPointerNotOverGameObject -= unitSpawnHolder.HidePreviewUnit;
        }

        private void HandleUnitSelected(ISelectableUnit selectable)
        {
            uiRoot.OnUnitSelected?.Invoke(selectable);
        }
        private void HandleUnitDeselected(ISelectableUnit selectable)
        {
            uiRoot.OnUnitDeselected?.Invoke(selectable);
        }
        private void HandleHoldStart(Vector3 vec)
        {
            uiRoot.OnHoldStarted?.Invoke(vec);
        }
        private void HandleHoldPerformed(Vector3 vec)
        {
            uiRoot.OnHoldPerformed?.Invoke(vec);
        }
        private void HandleHoldCanceled()
        {
            uiRoot.OnHoldCanceled?.Invoke();
        }
        private void HandleManageMenu()
        {
            uiRoot.OnManageMenu?.Invoke();
        }
    }
}
