using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.ControllUnit
{
    public class UnitMoveManager
    {
        private readonly SlotDestination slotDestination = new();
        
        public void RightClickMove(Vector3 destination, HashSet<ISelectableUnit> currentSelectedHash)
        {            
            foreach (var unit in currentSelectedHash)
            {
                Vector3 newDestination = slotDestination.GetSlotDestination(unit as Unit, destination, currentSelectedHash.Count);
                (unit as Unit).Controller.MoveTo(newDestination);
            }
        }
        
        public void ShiftRightClickMove(Vector3 destination, HashSet<ISelectableUnit> currentSelectedHash)
        {
            foreach (var unit in currentSelectedHash)
            {
                Vector3 newDestination = slotDestination.GetSlotDestination(unit as Unit, destination, currentSelectedHash.Count);
                (unit as Unit).Controller.MoveToReservation(newDestination);
            }
        }        
    }
}
