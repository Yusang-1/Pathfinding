using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public partial struct MoveCommendSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float3 worldPosition = float3.zero;
            
            foreach (var inputSystem in SystemAPI.Query<RefRO<MouseTargetData>>())
            {
                if (inputSystem.ValueRO.HasValue == false) continue;
                
                worldPosition = inputSystem.ValueRO.WorldPosition;
            }

            foreach (var unitData in SystemAPI.Query<RefRW<CrowdUnitData>>().WithAll<MoveUnitTag>())
            {
                unitData.ValueRW.Destination = worldPosition;
                unitData.ValueRW.HasDestination = true;
            }
        }
    }
}
