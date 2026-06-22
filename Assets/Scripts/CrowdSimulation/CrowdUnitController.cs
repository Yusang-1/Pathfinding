using UnityEngine;

namespace Assets.Scripts.CrowdSimulation
{
    public class CrowdUnitController
    {
        private CrowdUnit thisUnit;
        private SpatialHash spatialHash;
        private BoidsAlgorithm boidsAlgorithm;
        
        private Vector3 destination;
        private Vector3 velocity;
        private float speed;
        private bool hasDestination;

        public void Initialize(CrowdUnit unit, float speed, SpatialHash hash, BoidsAlgorithm boids)
        {
            thisUnit = unit;
            this.speed = speed;
            spatialHash = hash;
            spatialHash.AddUnit(thisUnit);
            boidsAlgorithm = boids;
        }

        public void ContorllerUpdate()
        {
            if (!hasDestination) return;
            
            Vector3 steeringVector = boidsAlgorithm.GetSteeringVector(thisUnit, spatialHash.GetUnitsInRange(thisUnit.transform.position, 1));
            
            Vector3 direction = (destination - thisUnit.transform.position + steeringVector).normalized;
            velocity = direction * speed;
            thisUnit.transform.position += velocity * Time.deltaTime;
            
            spatialHash.CheckUnitHash(thisUnit);
            
            if (CheckArrive())
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
            if (Vector3.SqrMagnitude(destination - thisUnit.transform.position) <= arriveThreshold)
            {
                return true;
            }
            else return false;
        }
    }
}

