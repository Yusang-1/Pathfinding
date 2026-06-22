using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class DestinationClusterManager
    {
        private readonly float formationRadius = 1.5f;

        public Vector3 GetAllocatedDestination(Vector3 baseDestination, ICollection<ISelectableUnit> units, int index)
        {
            if (units.Count <= 1) return baseDestination;

            // 원형 배치
            int unitIndex = index;
            float anglePerUnit = 360f / units.Count;
            float angle = unitIndex * anglePerUnit * Mathf.Deg2Rad;

            Vector3 offset = new(Mathf.Cos(angle) * formationRadius, Mathf.Sin(angle) * formationRadius, 0);

            return baseDestination + offset;
        }
    }
}
