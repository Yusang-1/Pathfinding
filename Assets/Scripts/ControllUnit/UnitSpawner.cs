using System;
using Assets.Scripts.ControllUnit.SO;
using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class UnitSpawner : AbstractSpawner
    {
        public event Action<ISelectableUnit> OnUnitSelected;
        public event Action<ISelectableUnit> OnUnitDeselected;
        public event Action<ActionMaps> OnSpawnAreaSettingStarted;

        [SerializeField] private UnitContainerSO unitContainerSO;
        [SerializeField] private Unit smallUnitPrefab;
        [SerializeField] private Unit largeUnitPrefab;
        [SerializeField] private UnitBottomSelectChanger unitBottomPrefab;
        [SerializeField] private Vector3 spawnPosition;        

        private UnitFactory unitFactory;
        private readonly SpawnAreaSetter spawnAreaSetter = new();
        
        private bool isInitialized;
        
        public void Initialize(UnitRuntimeContext unitRuntimeContext)
        {
            if(isInitialized) return;
            
            unitContainerSO.Initialize();
            unitFactory = new UnitFactory(unitBottomPrefab, unitRuntimeContext, unitContainerSO);

            unitFactory.OnSelectedCallback += HandleUnitSelected;
            unitFactory.OnDeselectedCallback += HandleUnitDeselected;
            spawnAreaSetter.OnStartSetSpawnAreaRequested += HandleSpawnAreaSettingStarted;
            
            isInitialized = true;
        }

        private void HandleUnitSelected(ISelectableUnit unit)
        {
            OnUnitSelected?.Invoke(unit);
        }

        private void HandleUnitDeselected(ISelectableUnit unit)
        {
            OnUnitDeselected?.Invoke(unit);
        }

        private void HandleSpawnAreaSettingStarted(ActionMaps actionMap)
        {
            OnSpawnAreaSettingStarted?.Invoke(actionMap);
        }

        public void StartSetSpawnArea(Action finishAction)
        {
            spawnAreaSetter.StartSetSpawnArea(finishAction);
        }

        public override void SpawnUnit(int unitCode)
        {
            unitFactory.SpawnUnit(unitCode, spawnPosition);
        }

        public override void SetSpawnArea(Vector3 position)
        {
            spawnPosition = position;
            spawnAreaSetter.FinishSetSpawnArea();
        }
    }
}
