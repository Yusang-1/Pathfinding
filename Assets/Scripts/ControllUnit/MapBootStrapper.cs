using UnityEngine;
using System;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class MapBootStrapper
    {
        private Action<ISelectableUnit> onSelectedHandler;
        private Action<ISelectableUnit> onDeselectedHandler;
        private Action<Vector3> onHoldStartedHandler;
        private Action<Vector3> onHoldPerformedHandler;
        private Action onHoldCanceledHandler;
        private Action onManageMenuHandler;

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

        public void Initialize(NodeData nodeData, MapRuntimeContext mapRuntimeContext, UnitsSO unitsSO, Pathfinder pathfinder)
        {            
            nodeData.Initialize();
            unitSpawner.Initialize(mapRuntimeContext, pathfinder, new UnitRuntimeContext(pathfinder, mapRuntimeContext.SpatialHash));
            inputManager.Initialize(selectableController);
            
            unitsSO.Initialize();
            
            InitializeHandlers();
        }
        
        private void InitializeHandlers()
        {
            onSelectedHandler = (selectable) => uiRoot.OnUnitSelected?.Invoke(selectable);
            onDeselectedHandler = (selectable) => uiRoot.OnUnitDeselected?.Invoke(selectable);
            onHoldStartedHandler = (vec) => uiRoot.OnHoldStarted?.Invoke(vec);
            onHoldPerformedHandler = (vec) => uiRoot.OnHoldPerformed?.Invoke(vec);
            onHoldCanceledHandler = () => uiRoot.OnHoldCanceled?.Invoke();
            onManageMenuHandler = () => uiRoot.OnManageMenu?.Invoke();
        }

        public void BindEvents(Action<MapData> InitializeMapRuntime, MapRuntimeContext mapRuntimeContext)
        {
            if (isBound) return; // 중복 이벤트 구독 방지

            AddUIRootEvent(InitializeMapRuntime, mapRuntimeContext);
            AddUnitSpawnerEvent();
            AddInputManagerEvent();

            isBound = true;
        }

        public void ResetBootStrapper(Action<MapData> InitializeMapRuntime, MapRuntimeContext mapRuntimeContext)
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
            unitSpawner.UnitFactory.OnSelectedCallback += onSelectedHandler;
            unitSpawner.UnitFactory.OnDeselectedCallback += onDeselectedHandler;
            unitSpawner.SpawnAreaSetter.OnStartSetSpawnAreaRequested += inputManager.ChangeActionMapSelected;
        }

        private void AddInputManagerEvent()
        {
            inputManager.OnHoldStarted += onHoldStartedHandler;
            inputManager.OnHoldPreformed += onHoldPerformedHandler;
            inputManager.OnHoldCanceled += onHoldCanceledHandler;
            inputManager.OnControllMenu += onManageMenuHandler;
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
            unitSpawner.UnitFactory.OnSelectedCallback -= onSelectedHandler;
            unitSpawner.UnitFactory.OnDeselectedCallback -= onDeselectedHandler;
            unitSpawner.SpawnAreaSetter.OnStartSetSpawnAreaRequested -= inputManager.ChangeActionMapSelected;
        }

        private void RemoveInputManagerEvent()
        {
            inputManager.OnHoldStarted -= onHoldStartedHandler;
            inputManager.OnHoldPreformed -= onHoldPerformedHandler;
            inputManager.OnHoldCanceled -= onHoldCanceledHandler;
            inputManager.OnControllMenu -= onManageMenuHandler;
            inputManager.OnSetSpawnAreaRequested -= unitSpawner.SetSpawnUnitArea;
        }
    }
}
