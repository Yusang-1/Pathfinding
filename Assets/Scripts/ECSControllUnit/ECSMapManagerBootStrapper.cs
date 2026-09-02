using UnityEngine;
using Unity.Entities;
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
        private readonly ECSSelectableController selectableController;
        private readonly MapdataJsonConverter mapdataJsonConverter = new();
        private readonly MapRuntimeContext mapRuntimeContext;
        private readonly ECSPathfindingBridge pathfindingBridge;

        private bool isEventBound;
        public ECSMapManagerBootStrapper(ControllUnitUIRoot uiRoot, ECSInputManager inputManager, ECSUnitSpawner unitSpawner,
            Action<MapData> initializeMapRuntime, MapRuntimeContext mapRuntimeContext, ECSPathfindingBridge pathfindingBridge, ECSSelectableController selectableController)
        {
            this.uiRoot = uiRoot;
            this.inputManager = inputManager;
            this.unitSpawner = unitSpawner;
            this.initializeMapRuntime = initializeMapRuntime;
            this.mapRuntimeContext = mapRuntimeContext;
            this.pathfindingBridge = pathfindingBridge;
            this.selectableController = selectableController;
        }

        public void Initialize(NodeData nodeData, UnitsSO unitsSO, PathfinderControllUnit pathfinder)
        {
            selectableController.Initialize();
            nodeData.Initialize();
            unitSpawner.Initialize();
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
            // uiRoot.OnFindSelectableUnitInDragUI += mapRuntimeContext.SpatialHash.GetUnitsInRange;
            // uiRoot.OnUnitFocused += selectableController.UnitFocusedList;
        }

        private void AddUnitSpawnerEvent()
        {
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
            
            selectableController.OnSelectedCallback += HandleUnitSelected;
            selectableController.OnDeselectedCallback += HandleUnitDeselected;
        }

        private void RemoveUIRootEvent(Action<MapData> SetMapData, MapRuntimeContext mapRuntimeContext)
        {
            uiRoot.OnLoadMapRequested -= SetMapData;
            uiRoot.OnGetOfficialMapListRequested -= mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested -= mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnSpawnUnitRequested -= unitSpawner.SpawnUnit;
            uiRoot.OnGetSpawnAreaRequested -= unitSpawner.StartSetSpawnArea;
            // uiRoot.OnFindSelectableUnitInDragUI -= mapRuntimeContext.SpatialHash.GetUnitsInRange;
            // uiRoot.OnUnitFocused -= selectableController.UnitFocusedList;
        }

        private void RemoveUnitSpawnerEvent()
        {
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
            
            selectableController.OnSelectedCallback -= HandleUnitSelected;
            selectableController.OnDeselectedCallback -= HandleUnitDeselected;
        }

        private void HandleUnitSelected(string name, Entity entity)
        {
            uiRoot.OnECSUnitSelected?.Invoke(name, entity);
        }
        private void HandleUnitDeselected(Entity entity)
        {
            uiRoot.OnECSUnitDeselected?.Invoke(entity);
        }
        private void HandleHoldStart(Vector3 vec)
        {
            uiRoot.OnHoldStarted?.Invoke(vec);
        }
        private Vector3? HandleHoldPerformed(Vector3 vec)
        {
            return uiRoot.OnECSHoldPerformed?.Invoke(vec);
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
