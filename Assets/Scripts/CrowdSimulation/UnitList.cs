using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.CrowdSimulation
{
    public class UnitList
    {
        private readonly List<CrowdUnit> units = new();
        private readonly BoidsAlgorithm boidsAlgorithm = new();
        
        public void AddUnit(CrowdUnit unit)
        {
            units.Add(unit);
            unit.Initialize(boidsAlgorithm);
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
