using Unity.Entities;

namespace Assets.Scripts.ECSControllUnit
{
    public partial struct UnitSpawnSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {            
            if (!SystemAPI.TryGetSingletonBuffer<UnitBySizeDynamicBuffer>(out var prefabBuffer, true))
            {
                return;
            }
            if(prefabBuffer.IsEmpty)
            {
                return;
            }

            // 생성 요청이 있는지 확인
            bool hasRequest = false;
            foreach (var a in SystemAPI.Query<UnitSpawnRequestComponent>())
            {
                hasRequest = true;
                break;
            }
            if (!hasRequest)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // UI가 보낸 '생성 요청 엔티티'들을 모두 탐색
            foreach (var (spawnRequest, spawnRequestEntity) in SystemAPI.Query<UnitSpawnRequestComponent>().WithEntityAccess())
            {
                // 원하는 실제 엔티티 생성 (복제)
                Entity prefab = Entity.Null;
                
                foreach(var prefabData in prefabBuffer)
                {
                    if(prefabData.Key == spawnRequest.UnitSize)
                    {
                        prefab = prefabData.Value;
                        break;
                    }
                }
                
                if(prefab == Entity.Null)
                {
                    ecb.DestroyEntity(spawnRequestEntity);
                    continue;
                }
                
                Entity newEntity = ecb.Instantiate(prefab);
                ecb.RemoveComponent<Disabled>(newEntity);

                // 한 번 처리한 요청 신호는 다음 프레임에 또 수행되지 않도록 삭제
                ecb.DestroyEntity(spawnRequestEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct UnitSpawnRequestComponent : IComponentData
    {
        public UnitSize UnitSize;
    }
}
