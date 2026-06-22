using UnityEngine;
using System.Collections.Generic;

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
}

