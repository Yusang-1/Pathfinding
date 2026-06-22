using UnityEngine;
using Assets.Scripts.CrowdSimulation.UI;

namespace Assets.Scripts.CrowdSimulation
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private CrowdUnit unitPrefab;
        [SerializeField] private UISpawnUnit uiSpawnUnit;

        private UnitList unitList;
        private SpatialHash spatialHash;

        private void Start()
        {
            uiSpawnUnit.OnSpawnUnitRequested += SpawnUnit;
        }
        
        public void Initialize(UnitList unitList, SpatialHash hash)
        {
            this.unitList = unitList;
            spatialHash = hash;
        }

        public void SpawnUnit()
        {
            var unit = Instantiate<CrowdUnit>(unitPrefab);
            unit.UnitSpawned(spatialHash);
            unitList.AddUnit(unit);            
        }
    }
}
