using UnityEngine;

namespace Assets.Scripts.CrowdSimulation
{
    public class CrowdSimulationManager : MonoBehaviour
    {
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private UnitInput unitInput;
        
        private void Start()
        {
            UnitList unitList = new();
            unitSpawner.Initialize(unitList);
            unitInput.Initialize(unitList);
        }
    }
}
