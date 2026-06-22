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
    }
}
