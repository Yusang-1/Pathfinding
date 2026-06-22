using UnityEngine;
using Assets.Scripts.CrowdSimulation.SO;

namespace Assets.Scripts.CrowdSimulation
{
    public class CrowdUnit : MonoBehaviour
    {        
        [SerializeField] private CrowdUnitSO unitData;
        
        private readonly CrowdUnitController controller = new();

        private void Start()
        {
            controller.Initialize(this, unitData.Speed);            
        }
        
        private void Update()
        {
            controller.ContorllerUpdate();
        }
        
        public void MoveUnit(Vector3 destination)
        {
            controller.SetDestination(destination);
        }
    }
}
