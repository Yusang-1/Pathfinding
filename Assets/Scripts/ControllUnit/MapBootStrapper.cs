using UnityEngine;
using System;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;
using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.ControllUnit
{
    public class MapBootStrapper
    {
        private readonly SelectableController selectableController = new();
        private readonly MapdataJsonConverter mapdataJsonConverter = new();

        private readonly ControllUnitUIRoot uiRoot;
        private readonly InputManager inputManager;
        private readonly UnitSpawner unitSpawner;

        private bool isBound;

        public MapBootStrapper(ControllUnitUIRoot uiRoot, InputManager inputManager, UnitSpawner unitSpawner)
        {
            this.uiRoot = uiRoot;            
            this.inputManager = inputManager;
            this.unitSpawner = unitSpawner;
        }

        public void Initialize(NodeData nodeData, MapRuntimeContext mapRuntimeContext, UnitsSO unitsSO, PathfinderControllUnit pathfinder)
        {            
            nodeData.Initialize();
            unitSpawner.Initialize(new UnitRuntimeContext(pathfinder, mapRuntimeContext.SpatialHash));
            inputManager.Initialize(selectableController);
            
            unitsSO.Initialize();
        }                
        
        public void BindEvents(Action<MapData> InitializeMapRuntime, MapRuntimeContext mapRuntimeContext)
        {
            if (isBound) return; // 중복 이벤트 구독 방지

            AddUIRootEvent(InitializeMapRuntime, mapRuntimeContext);
            AddUnitSpawnerEvent();
            AddInputManagerEvent();

            isBound = true;
        }

        public void UnbindEvents(Action<MapData> InitializeMapRuntime, MapRuntimeContext mapRuntimeContext)
        {
            if (!isBound) return;

            RemoveUIRootEvent(InitializeMapRuntime, mapRuntimeContext);
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
            uiRoot.OnGetSpawnAreaRequested += unitSpawner.SpawnAreaSetter.StartSetSpawnArea;
            uiRoot.OnFindSelectableUnitInDragUI += mapRuntimeContext.SpatialHash.GetUnitsInRange;
            uiRoot.OnUnitFocused += selectableController.UnitFocusedList;
        }

        private void AddUnitSpawnerEvent()
        {
            unitSpawner.UnitFactory.OnSelectedCallback += HandleUnitSelected;
            unitSpawner.UnitFactory.OnDeselectedCallback += HandleUnitDeselected;
            unitSpawner.SpawnAreaSetter.OnStartSetSpawnAreaRequested += inputManager.ChangeActionMapSelected;
        }

        private void AddInputManagerEvent()
        {
            inputManager.OnHoldStarted += HandleHoldStart;
            inputManager.OnHoldPerformed += HandleHoldPerformed;
            inputManager.OnHoldCanceled += HandleHoldCanceled;
            inputManager.OnControllMenu += HandleManageMenu;
            inputManager.OnSetSpawnAreaRequested += unitSpawner.SetSpawnUnitArea;
        }

        private void RemoveUIRootEvent(Action<MapData> SetMapData, MapRuntimeContext mapRuntimeContext)
        {
            uiRoot.OnLoadMapRequested -= SetMapData;
            uiRoot.OnGetOfficialMapListRequested -= mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested -= mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnSpawnUnitRequested -= unitSpawner.SpawnUnit;
            uiRoot.OnGetSpawnAreaRequested -= unitSpawner.SpawnAreaSetter.StartSetSpawnArea;
            uiRoot.OnFindSelectableUnitInDragUI -= mapRuntimeContext.SpatialHash.GetUnitsInRange;
            uiRoot.OnUnitFocused -= selectableController.UnitFocusedList;
        }

        private void RemoveUnitSpawnerEvent()
        {
            unitSpawner.UnitFactory.OnSelectedCallback -= HandleUnitSelected;
            unitSpawner.UnitFactory.OnDeselectedCallback -= HandleUnitDeselected;
            unitSpawner.SpawnAreaSetter.OnStartSetSpawnAreaRequested -= inputManager.ChangeActionMapSelected;
        }

        private void RemoveInputManagerEvent()
        {
            inputManager.OnHoldStarted -= HandleHoldStart;
            inputManager.OnHoldPerformed -= HandleHoldPerformed;
            inputManager.OnHoldCanceled -= HandleHoldCanceled;
            inputManager.OnControllMenu -= HandleManageMenu;
            inputManager.OnSetSpawnAreaRequested -= unitSpawner.SetSpawnUnitArea;
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
