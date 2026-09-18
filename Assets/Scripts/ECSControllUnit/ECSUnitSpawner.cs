using UnityEngine;
using System;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSUnitSpawner : AbstractSpawner
    {
        public event Action<ActionMaps> OnSpawnAreaSettingStarted;

        [SerializeField] private Vector3 spawnPosition;

        private readonly ECSUnitFactory unitFactory = new();
        private readonly SpawnAreaSetter spawnAreaSetter = new();

        private bool isInitialized;

        public void Initialize()
        {
            if (isInitialized) return;

            spawnAreaSetter.OnStartSetSpawnAreaRequested += HandleSpawnAreaSettingStarted;

            isInitialized = true;
        }

        public override void SpawnUnit(int unitCode)
        {
            unitFactory.MakeUnitSpawnRequest(unitCode, spawnPosition);
        }
        
        public override void SpawnUnit(int unitCode, Vector3 position)
        {
            unitFactory.MakeUnitSpawnRequest(unitCode, position);
        }

        public void StartSetSpawnArea(Action finishAction)
        {
            spawnAreaSetter.StartSetSpawnArea(finishAction);
        }

        private void HandleSpawnAreaSettingStarted(ActionMaps actionMap)
        {
            OnSpawnAreaSettingStarted?.Invoke(actionMap);
        }
    }
}

