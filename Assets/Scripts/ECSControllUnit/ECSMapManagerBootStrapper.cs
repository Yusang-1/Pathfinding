using UnityEngine;
using System;
using Assets.Scripts.ControllUnit;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;
using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSMapManagerBootStrapper
    {
        private readonly Action<MapData> initializeMapRuntime;

        private readonly ControllUnitUIRoot uiRoot;
        private readonly ECSInputManager inputManager;
        private readonly ECSUnitSpawner unitSpawner;
        private readonly ECSSelectableController selectableController = new();
        private readonly MapdataJsonConverter mapdataJsonConverter = new();
        private readonly MapRuntimeContext mapRuntimeContext;
        private readonly ECSPathfindingBridge pathfindingBridge;

        private bool isEventBound;
        public ECSMapManagerBootStrapper(ControllUnitUIRoot uiRoot, ECSInputManager inputManager, ECSUnitSpawner unitSpawner,
            Action<MapData> initializeMapRuntime, MapRuntimeContext mapRuntimeContext, ECSPathfindingBridge pathfindingBridge)
        {
            this.uiRoot = uiRoot;
            this.inputManager = inputManager;
            this.unitSpawner = unitSpawner;
            this.initializeMapRuntime = initializeMapRuntime;
            this.mapRuntimeContext = mapRuntimeContext;
            this.pathfindingBridge = pathfindingBridge;
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
            if (isEventBound) return;

            AddUIRootEvent(initializeMapRuntime, mapRuntimeContext);
            AddUnitSpawnerEvent();
            AddInputManagerEvent();
            AddNodeListEvent();
            AddSelectableControllerEvent();

            isEventBound = true;
        }

        public void UnbindEvents()
        {
            if (!isEventBound) return;

            RemoveUIRootEvent(initializeMapRuntime, mapRuntimeContext);
            RemoveUnitSpawnerEvent();
            RemoveInputManagerEvent();
            RemoveNodeListEvent();
            RemoveSelectableControllerEvent();

            isEventBound = false;
        }

        private void AddUIRootEvent(Action<MapData> SetMapData, MapRuntimeContext mapRuntimeContext)
        {
            uiRoot.OnLoadMapRequested += SetMapData;
            uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnSpawnUnitRequested += unitSpawner.SpawnUnit;
            uiRoot.OnGetSpawnAreaRequested += unitSpawner.StartSetSpawnArea;
            uiRoot.OnFindSelectableUnitInDragUI += mapRuntimeContext.SpatialHash.GetUnitsInRange;
            // uiRoot.OnUnitFocused += selectableController.UnitFocusedList;
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

        private void AddNodeListEvent()
        {
            mapRuntimeContext.NodeList.NodeTypeController.NodeTypeDrawer.OnPathfindAvailable += HandlePathfindAvailable;
        }
        
        private void AddSelectableControllerEvent()
        {
            selectableController.OnMove += pathfindingBridge.Move;
            selectableController.OnMoveAdditive += pathfindingBridge.MoveAdditive;
        }

        private void RemoveUIRootEvent(Action<MapData> SetMapData, MapRuntimeContext mapRuntimeContext)
        {
            uiRoot.OnLoadMapRequested -= SetMapData;
            uiRoot.OnGetOfficialMapListRequested -= mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested -= mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnSpawnUnitRequested -= unitSpawner.SpawnUnit;
            uiRoot.OnGetSpawnAreaRequested -= unitSpawner.StartSetSpawnArea;
            uiRoot.OnFindSelectableUnitInDragUI -= mapRuntimeContext.SpatialHash.GetUnitsInRange;
            // uiRoot.OnUnitFocused -= selectableController.UnitFocusedList;
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
            inputManager.OnSetSpawnAreaRequested -= unitSpawner.SetSpawnArea;
        }

        private void RemoveNodeListEvent()
        {
            mapRuntimeContext.NodeList.NodeTypeController.NodeTypeDrawer.OnPathfindAvailable -= HandlePathfindAvailable;
        }
        
        private void RemoveSelectableControllerEvent()
        {
            selectableController.OnMove -= pathfindingBridge.Move;
            selectableController.OnMoveAdditive -= pathfindingBridge.MoveAdditive;
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
        private void HandlePathfindAvailable(bool value)
        {
            return;
        }
    }
}
