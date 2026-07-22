using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class SlotDestination
    {
        private readonly Dictionary<int, int> slotIndexByUnit = new();
        
        public Vector3 GetSlotDestination(Unit unit, Vector3 center, int totalUnitCount)
        {
            int id = unit.GetInstanceID();

            if (!slotIndexByUnit.ContainsKey(id))
            {
                slotIndexByUnit[id] = slotIndexByUnit.Count;
            }

            int slotIndex = slotIndexByUnit[id];
            int count = Mathf.Max(1, totalUnitCount);

            float angle = slotIndex % count * (2f * Mathf.PI / count);
            float slotRadius = unit.UnitData.Radius * 2f + 0.3f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * slotRadius;

            Debug.Log($"{unit.GetInstanceID()} {slotIndex} {count} {center + offset}");
            return center + offset;
        }
    }
}
