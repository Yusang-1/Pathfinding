using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.ECS
{
    public struct MouseTargetData : IComponentData
    {
        public float3 WorldPosition;
        public bool HasValue;
    }
}
