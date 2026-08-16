using UnityEngine;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private Unit smallUnitPrefab;
        [SerializeField] private Unit largeUnitPrefab;
        [SerializeField] private UnitBottomSelectChanger unitBottomPrefab;
        [SerializeField] private Vector3 spawnPosition;

        private UnitFactory unitFactory;
        private readonly SpawnAreaSetter spawnAreaSetter = new();

        public SpawnAreaSetter SpawnAreaSetter => spawnAreaSetter;
        public UnitFactory UnitFactory => unitFactory;

        public void Initialize(UnitRuntimeContext unitRuntimeContext)
        {
            unitFactory = new UnitFactory(smallUnitPrefab, largeUnitPrefab, unitBottomPrefab, unitRuntimeContext);
        }

        public void SpawnUnit(UnitSize unitSize)
        {
            unitFactory.SpawnUnit(unitSize, spawnPosition);
        }

        public void SetSpawnUnitArea(Vector3 position)
        {
            spawnPosition = position;
            spawnAreaSetter.SetFinishAction?.Invoke();
        }
    }
}
