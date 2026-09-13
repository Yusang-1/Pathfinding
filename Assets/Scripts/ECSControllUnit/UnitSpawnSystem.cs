using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.ECSControllUnit
{
    public partial struct UnitSpawnSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonBuffer<UnitByCodeDynamicBuffer>(out var prefabBuffer, true))
            {
                return;
            }
            if (prefabBuffer.IsEmpty)
            {
                return;
            }

            if (!SystemAPI.TryGetSingleton<UnitBottomContainer>(out var bottomContainer))
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

                foreach (var prefabData in prefabBuffer)
                {
                    if (prefabData.Code == spawnRequest.UnitCode)
                    {
                        prefab = prefabData.Value;
                        break;
                    }
                }

                if (prefab == Entity.Null)
                {
                    ecb.DestroyEntity(spawnRequestEntity);
                    continue;
                }

                Entity newEntity = ecb.Instantiate(prefab);
                Entity newBottomPrefabSelected = state.EntityManager.Instantiate(bottomContainer.SelectPrefab);
                Entity newBottomPrefabFocused = state.EntityManager.Instantiate(bottomContainer.FocusPrefab);

                ECSUnitComponent defaultComponent = state.EntityManager.GetComponentData<ECSUnitComponent>(prefab);

                // Bottom 컴포넌트 설정
                LocalTransform selectedBottomTransform = state.EntityManager.GetComponentData<LocalTransform>(newBottomPrefabSelected);
                selectedBottomTransform.Position = spawnRequest.Position;
                selectedBottomTransform.Scale = defaultComponent.Radius * 2f;
                ecb.SetComponent(newBottomPrefabSelected, selectedBottomTransform);

                LocalTransform focusedBottomTransform = state.EntityManager.GetComponentData<LocalTransform>(newBottomPrefabFocused);
                focusedBottomTransform.Position = spawnRequest.Position;
                focusedBottomTransform.Scale = defaultComponent.Radius * 2f;
                ecb.SetComponent(newBottomPrefabFocused, focusedBottomTransform);

                // Entity 컴포넌트 설정
                ecb.RemoveComponent<Disabled>(newEntity);
                LocalTransform unitTransform = state.EntityManager.GetComponentData<LocalTransform>(prefab);
                unitTransform.Position = spawnRequest.Position;
                ecb.SetComponent(newEntity, unitTransform);
                ecb.SetComponent(newEntity,
                    new ECSUnitComponent
                    {
                        UnitName = defaultComponent.UnitName,
                        Radius = defaultComponent.Radius,
                        BottomCircleSelected = newBottomPrefabSelected,
                        BottomCircleFocused = newBottomPrefabFocused
                        // IconName = authoring.unitData.UnitIcon.name
                    }
                );

                // 한 번 처리한 요청 신호는 다음 프레임에 또 수행되지 않도록 삭제
                ecb.DestroyEntity(spawnRequestEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct UnitSpawnRequestComponent : IComponentData
    {
        public int UnitCode;
        public float3 Position;
    }
}
