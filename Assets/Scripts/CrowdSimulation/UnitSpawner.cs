using UnityEngine;
using Assets.Scripts.CrowdSimulation.UI;

namespace Assets.Scripts.CrowdSimulation
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private CrowdUnit unitPrefab;
        [SerializeField] private UISpawnUnit uiSpawnUnit;
        
        private readonly UnitList unitList = new();

        private void Start()
        {
            uiSpawnUnit.OnSpawnUnitRequested += SpawnUnit;
        }
        
        public void SpawnUnit()
        {
            var unit = Instantiate<CrowdUnit>(unitPrefab);
            unitList.AddUnit(unit);
        }
    }
}
