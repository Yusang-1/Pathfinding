using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.ECS
{
    public struct CrowdUnitData : IComponentData
    {
        public float Speed;
        public float Radius;
        public bool HasDestination;
        public float3 Destination;
        public float3 Velocity;
    }
}
