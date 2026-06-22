using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.CrowdSimulation
{
    public class UnitList
    {
        private readonly List<CrowdUnit> units = new();
        
        public void AddUnit(CrowdUnit unit)
        {
            units.Add(unit);
        }
        
        public void MoveUnits(Vector2 destination)
        {
            foreach(var unit in units)
            {
                unit.MoveUnit(destination);
            }
        }
    }
}
