using UnityEngine;

namespace Assets.Scripts.CrowdSimulation
{
    public class CrowdSimulationManager : MonoBehaviour
    {
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private UnitInput unitInput;                
        
        private void Start()
        {
            SpatialHash spatialHash = new();
            UnitList unitList = new();
            unitSpawner.Initialize(unitList, spatialHash);
            unitInput.Initialize(unitList);
        }
    }
}
