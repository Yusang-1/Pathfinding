using Unity.Entities;

namespace Assets.Scripts.ECS
{
    public struct NearbyEntityElement : IBufferElementData
    {
        public Entity Value;
    }
}
