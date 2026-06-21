using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class UnitCrowdControl
    {
        private readonly DestinationClusterManager destinationClusterManager = new();

        public void MoveCrowd(Vector3 destination, HashSet<ISelectableUnit> units)
        {
            int count = 0;
            foreach (var unit in units)
            {
                var allocatedDestination = destinationClusterManager.GetAllocatedDestination(destination, units, count++);
                (unit as Unit).Controller.MoveTo(allocatedDestination);
            }
        }
        
        public void MoveCrowdReservation(Vector3 destination, HashSet<ISelectableUnit> units)
        {
            int count = 0;
            foreach (var unit in units)
            {
                var allocatedDestination = destinationClusterManager.GetAllocatedDestination(destination, units, count++);
                (unit as Unit).Controller.MoveToReservation(allocatedDestination);
            }
        }
    }

    public class DestinationClusterManager
    {
        private readonly float formationRadius = 1.5f;
        private readonly float minDistanceBetweenUnits = 0.5f;

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

