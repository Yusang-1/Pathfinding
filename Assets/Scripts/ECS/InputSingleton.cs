using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.ECS
{
    public static class InputSingleton
    {
        public static Entity Entity;

        public static void Create(World world)
        {
            var entityManager = world.EntityManager;

            Entity = entityManager.CreateEntity(typeof(MouseTargetData));
            entityManager.SetComponentData(Entity, new MouseTargetData
            {
                WorldPosition = float3.zero,
                HasValue = false
            });
        }

        public static void Set(float3 worldPos, bool value)
        {
            if (Entity == Entity.Null) return;
        
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;
            
            entityManager.SetComponentData(Entity, new MouseTargetData
            {
                WorldPosition = worldPos,
                HasValue = value                
            });
        }
    }
}
