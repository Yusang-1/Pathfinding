using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.CrowdSimulation
{
    public class SteeringBehavior
    {
        public Vector3 GetSteering(CrowdUnit unit, List<CrowdUnit> nearby, float maxSpeed, Vector3 destination, SteeringConfig weighting)
        {
            var seekVector = Seek(unit.transform.position, destination, maxSpeed, unit.Controller.Velocity);
            seekVector *= weighting.SeekWeight;

            if (nearby == null || nearby.Count == 1) return seekVector;

            var separationVector = Separation(unit, nearby, 0.7f);
            separationVector *= weighting.SeparationWeight;

            var cohesionVector = Cohesion(unit, nearby, maxSpeed);
            cohesionVector *= weighting.CohesionWeight;

            var alignmentVector = Alignment(unit, nearby);
            alignmentVector *= weighting.AlignmentWeight;

            return seekVector + separationVector + cohesionVector + alignmentVector;
        }

        private Vector3 Seek(Vector3 position, Vector3 target, float maxSpeed, Vector3 velocity)
        {
            Vector3 desired = (target - position).normalized * maxSpeed;
            return desired - velocity;
        }

        private Vector3 Separation(CrowdUnit unit, List<CrowdUnit> nearby, float separationRadius)
        {
            Vector3 steeringForce = Vector3.zero;

            foreach (var other in nearby)
            {
                if (other == unit) continue;
                
                float distance = Vector3.Distance(unit.transform.position, other.transform.position);
                if (distance < separationRadius && distance > 0.01f)
                {
                    Vector3 diff = (unit.transform.position - other.transform.position).normalized;
                    diff /= distance;
                    steeringForce += diff;
                }
            }

            return steeringForce;
        }

        private Vector3 Cohesion(CrowdUnit unit, List<CrowdUnit> nearby, float maxSpeed)
        {
            Vector3 centerOfMass = Vector3.zero;
            Vector3 unitPosition = unit.transform.position;

            foreach (var other in nearby)
            {
                if (other == unit) continue;

                centerOfMass += other.transform.position;
            }

            centerOfMass /= nearby.Count;
            return Seek(unitPosition, centerOfMass, maxSpeed, unit.Controller.Velocity);
        }

        private Vector3 Alignment(CrowdUnit unit, List<CrowdUnit> nearby)
        {
            Vector3 averageVelocity = Vector3.zero;

            foreach (var other in nearby)
            {
                if (other == unit) continue;
                
                averageVelocity += other.Controller.Velocity;
            }
            return averageVelocity /= nearby.Count;
        }
    }
}

