using UnityEngine;
using System;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSUnitSpawner : AbstractSpawner
    {
        public event Action<ActionMaps> OnSpawnAreaSettingStarted;

        [SerializeField] private Vector3 spawnPosition;

        private ECSUnitFactory unitFactory;        

        private readonly SpawnAreaSetter spawnAreaSetter = new();

        private bool isInitialized;

        public void Initialize(UnitRuntimeContext unitRuntimeContext)
        {
            if (isInitialized) return;

            unitFactory = new ECSUnitFactory(unitRuntimeContext);

            spawnAreaSetter.OnStartSetSpawnAreaRequested += HandleSpawnAreaSettingStarted;

            isInitialized = true;
        }

        public override void SpawnUnit(UnitSize unitSize) // UnitSize unitSize
        {
            unitFactory.SpawnUnit(unitSize, spawnPosition);
        }

        public void StartSetSpawnArea(Action finishAction)
        {
            spawnAreaSetter.StartSetSpawnArea(finishAction);
        }

        public override void SetSpawnArea(Vector3 position)
        {
            // spawnPosition = position;
            // spawnAreaSetter.SetFinishAction?.Invoke();
        }

        private void HandleSpawnAreaSettingStarted(ActionMaps actionMap)
        {
            OnSpawnAreaSettingStarted?.Invoke(actionMap);
        }
    }
}

