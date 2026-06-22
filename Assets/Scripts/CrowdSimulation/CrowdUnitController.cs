using UnityEngine;

namespace Assets.Scripts.CrowdSimulation
{
    public class CrowdUnitController
    {
        private CrowdUnit thisUnit;
        
        private Vector3 destination;
        private Vector3 velocity;
        private float speed;
        private bool hasDestination;
        
        public void Initialize(CrowdUnit unit, float speed)
        {
            thisUnit = unit;
            this.speed = speed;
        }
        
        public void ContorllerUpdate()
        {
            if(!hasDestination) return;
            
            Vector3 direction = (destination - thisUnit.transform.position).normalized;
            velocity = direction * speed;
            thisUnit.transform.position += velocity * Time.deltaTime;
            
            if(CheckArrive())
            {                
                hasDestination = false;
            }
        }
        
        public void SetDestination(Vector3 destination)
        {
            this.destination = destination;
            hasDestination = true;
        }
        
        private const float arriveThreshold = 0.01f;
        private bool CheckArrive()
        {
            if(Vector3.SqrMagnitude(destination - thisUnit.transform.position) <= arriveThreshold)
            {
                return true;
            }
            else return false;
        }
    }
}

