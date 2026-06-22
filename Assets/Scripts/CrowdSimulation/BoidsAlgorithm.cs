using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.CrowdSimulation
{
    public class BoidsAlgorithm
    {
        public Vector3 GetSteeringVector(CrowdUnit unit, List<CrowdUnit> nearUnits)
        {
            Vector3 saparationVector = Saparation(unit, nearUnits, 0.7f);
            Vector3 alignmentVector = Alignment(nearUnits);
            Vector3 cohesionVector = Cohesion(unit, nearUnits);
            
            return saparationVector + alignmentVector + cohesionVector;
        }

        private Vector3 Saparation(CrowdUnit unit, List<CrowdUnit> nearUnits, float separationRadius)
        {
            Vector3 steeringForce = Vector3.zero;

            foreach (CrowdUnit other in nearUnits)
            {
                float distance = Vector3.Distance(unit.transform.position, other.transform.position);
                if (distance < separationRadius && distance > 0.01f)
                {
                    // 다른 유닛으로부터 반대 방향
                    Vector3 diff = (unit.transform.position - other.transform.position).normalized;
                    diff /= distance; // 거리에 따라 감소
                    steeringForce += diff;
                }
            }

            return steeringForce.normalized;
        }

        private Vector3 Alignment(List<CrowdUnit> nearUnits)
        {
            Vector3 result = new();
            
            foreach (var other in nearUnits)
            {
                result += other.transform.forward;
            }

            return (result /= nearUnits.Count).normalized;
        }

        private Vector3 Cohesion(CrowdUnit unit, List<CrowdUnit> nearUnits)
        {
            Vector3 result = new();

            foreach (var other in nearUnits)
            {
                result += other.transform.position;
            }
            result /= nearUnits.Count;

            return result.normalized;
        }
    }
}
