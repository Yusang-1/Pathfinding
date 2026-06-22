using UnityEngine;
using Assets.Scripts.CrowdSimulation.SO;

namespace Assets.Scripts.CrowdSimulation
{
    public class CrowdUnit : MonoBehaviour
    {        
        [SerializeField] private CrowdUnitSO unitData;
        
        private readonly CrowdUnitController controller = new();
        private BoidsAlgorithm boidsAlgorithm;
        
        public Vector2Int CurrentKey;   
        
        private void Update()
        {
            controller.ContorllerUpdate();
        }
        
        public void Initialize(BoidsAlgorithm boidsAlgorithm)
        {
            this.boidsAlgorithm = boidsAlgorithm;
        }
        
        public void UnitSpawned(SpatialHash spatialHash)
        {
            controller.Initialize(this, unitData.Speed, spatialHash, boidsAlgorithm);
        }
        
        public void MoveUnit(Vector3 destination)
        {
            controller.SetDestination(destination);
        }
    }
}
