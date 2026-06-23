using UnityEngine;
using Assets.Scripts.CrowdSimulation.SO;

namespace Assets.Scripts.CrowdSimulation
{
    public class CrowdUnit : MonoBehaviour
    {        
        [SerializeField] private CrowdUnitSO unitData;
        
        private readonly CrowdUnitController controller = new();
        
        public Vector2Int CurrentKey;   
        
        public CrowdUnitController Controller => controller;
        
        private void Update()
        {
            controller.ContorllerUpdate();
        }
        
        public void UnitSpawned(SpatialHash spatialHash)
        {
            controller.Initialize(this, unitData.Speed, spatialHash);
        }
        
        public void MoveUnit(Vector3 destination)
        {
            controller.SetDestination(destination);
        }
    }
}
