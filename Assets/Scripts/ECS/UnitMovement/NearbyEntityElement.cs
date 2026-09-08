using Unity.Entities;

namespace Assets.Scripts.ECS.UnitMovement
{
    public struct NearbyEntityElement : IBufferElementData
    {
        public Entity Value;
    }
}
