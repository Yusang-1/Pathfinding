using Unity.Entities;

namespace Assets.Scripts.ECS
{
    public struct ECSCrowdUnitSpawnerData : IComponentData
    {
        public Entity EntityPrefab;
    }

    public struct SpawnRequestData : IComponentData { }

    public partial struct ECSCrowdUnitSpawnSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // 스포너(프리팹 정보) 가져오기
            if (!SystemAPI.TryGetSingleton<ECSCrowdUnitSpawnerData>(out var spawner))
            {
                return;
            }

            // 1. UI가 보낸 '생성 요청 엔티티'들을 모두 탐색
            foreach (var (request, spawnRequestEntity) in SystemAPI.Query<RefRO<SpawnRequestData>>().WithEntityAccess())
            {
                if (spawner.EntityPrefab == Entity.Null)
                {
                    UnityEngine.Debug.Log("SpawnRequestData is Null");
                    continue;
                }

                // 2. 원하는 실제 엔티티 생성 (복제)
                Entity newEntity = ecb.Instantiate(spawner.EntityPrefab);
                ecb.RemoveComponent(newEntity, typeof(Disabled));
                
                // 3. 한 번 처리한 요청 신호는 다음 프레임에 또 수행되지 않도록 삭제
                ecb.DestroyEntity(spawnRequestEntity);

                // EntityGameObjectManager.CreateNewEntity(newEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
