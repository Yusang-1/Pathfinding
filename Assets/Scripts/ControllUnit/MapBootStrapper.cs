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
        private readonly Action<MapData> initializeMapRuntime;
        
        private readonly SelectableController selectableController = new();
        private readonly MapdataJsonConverter mapdataJsonConverter = new();

        private readonly ControllUnitUIRoot uiRoot;
        private readonly InputManager inputManager;
        private readonly UnitSpawner unitSpawner;
        private readonly MapRuntimeContext mapRuntimeContext;

        private bool isBound;

        public MapBootStrapper(ControllUnitUIRoot uiRoot, InputManager inputManager, UnitSpawner unitSpawner, Action<MapData> initializeMapRuntime, MapRuntimeContext mapRuntimeContext)
        {
            this.uiRoot = uiRoot;
            this.inputManager = inputManager;
            this.unitSpawner = unitSpawner;
            this.initializeMapRuntime= initializeMapRuntime;
            this.mapRuntimeContext = mapRuntimeContext;
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

        private void AddUIRootEvent(Action<MapData> SetMapData, MapRuntimeContext mapRuntimeContext)
        {
            uiRoot.OnLoadMapRequested += SetMapData;
            uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnSpawnUnitRequested += unitSpawner.SpawnUnit;
            uiRoot.OnGetSpawnAreaRequested += unitSpawner.StartSetSpawnArea;
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
            inputManager.OnSetSpawnAreaRequested += unitSpawner.SetSpawnArea;
        }

        private void RemoveUIRootEvent(Action<MapData> SetMapData, MapRuntimeContext mapRuntimeContext)
        {
            uiRoot.OnLoadMapRequested -= SetMapData;
            uiRoot.OnGetOfficialMapListRequested -= mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested -= mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnSpawnUnitRequested -= unitSpawner.SpawnUnit;
            uiRoot.OnGetSpawnAreaRequested -= unitSpawner.StartSetSpawnArea;
            uiRoot.OnFindSelectableUnitInDragUI -= mapRuntimeContext.SpatialHash.GetUnitsInRange;
            uiRoot.OnUnitFocused -= selectableController.UnitFocusedList;
        }

        private void RemoveUnitSpawnerEvent()
        {
            unitSpawner.OnUnitSelected += HandleUnitSelected;
            unitSpawner.OnUnitDeselected += HandleUnitDeselected;
            unitSpawner.OnSpawnAreaSettingStarted += inputManager.ChangeActionMapSelected;
        }

        private void RemoveInputManagerEvent()
        {
            inputManager.OnHoldStarted -= HandleHoldStart;
            inputManager.OnHoldPerformed -= HandleHoldPerformed;
            inputManager.OnHoldCanceled -= HandleHoldCanceled;
            inputManager.OnControllMenu -= HandleManageMenu;
            inputManager.OnSetSpawnAreaRequested -= unitSpawner.SetSpawnArea;
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
