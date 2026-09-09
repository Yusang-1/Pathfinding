using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.ControllUnit;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Pathfinding
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
        
        private readonly Dictionary<Entity, int> slotIndexByEntity = new();
        
        public float3 GetSlotDestination(Entity entity, float3 center, int totalUnitCount, float unitRadius)
        {
            if (!slotIndexByEntity.ContainsKey(entity))
            {
                slotIndexByEntity[entity] = slotIndexByEntity.Count;
            }

            int slotIndex = slotIndexByEntity[entity];
            int count = Mathf.Max(1, totalUnitCount);

            float angle = slotIndex % count * (2f * Mathf.PI / count);
            float slotRadius = unitRadius * 2f + 0.3f;
            float3 offset = new float3(math.cos(angle), 0f, math.sin(angle)) * slotRadius;

            Debug.Log($"{entity} {slotIndex} {count} {center + offset}");
            return center + offset;
        }
    }
}
