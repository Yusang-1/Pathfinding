using UnityEngine;
using Assets.Scripts.CrowdSimulation.SO;

namespace Assets.Scripts.CrowdSimulation
{
    public class CrowdUnitController
    {
        private CrowdUnit thisUnit;
        private SpatialHash spatialHash;
        private SteeringWeightingSO steeringWeightingData;
        private readonly SteeringBehavior steeringBehavior = new();

        private Vector3 destination;
        private Vector3 velocity;
        private float speed;
        private bool hasDestination;

        public Vector3 Velocity => velocity;

        public void Initialize(CrowdUnit unit, float speed, SpatialHash hash, SteeringWeightingSO weightingData)
        {
            thisUnit = unit;
            this.speed = speed;
            spatialHash = hash;
            spatialHash.AddUnit(thisUnit);
            steeringWeightingData = weightingData;
        }

        public void ContorllerUpdate()
        {
            if (!hasDestination) return;
            
            var nearby = spatialHash.GetUnitsInRange(thisUnit.transform.position, 1);
            Vector3 steering = steeringBehavior.GetSteering(thisUnit, nearby, speed, destination, steeringWeightingData.WalkConfig);
            velocity = steering * Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, speed);
                        
            thisUnit.transform.position += velocity;

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

