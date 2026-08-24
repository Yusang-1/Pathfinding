using System;
using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class UnitSpawner : AbstractSpawner
    {
        public event Action<ISelectableUnit> OnUnitSelected;
        public event Action<ISelectableUnit> OnUnitDeselected;
        public event Action<string> OnSpawnAreaSettingStarted;

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
            
            unitFactory = new UnitFactory(smallUnitPrefab, largeUnitPrefab, unitBottomPrefab, unitRuntimeContext);

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

        private void HandleSpawnAreaSettingStarted(string actionMapName)
        {
            OnSpawnAreaSettingStarted?.Invoke(actionMapName);
        }

        public void StartSetSpawnArea(Action finishAction)
        {
            spawnAreaSetter.StartSetSpawnArea(finishAction);
        }

        public override void SpawnUnit(UnitSize unitSize)
        {
            unitFactory.SpawnUnit(unitSize, spawnPosition);
        }

        public override void SetSpawnArea(Vector3 position)
        {
            spawnPosition = position;
            spawnAreaSetter.FinishSetSpawnArea();
        }
    }
}
