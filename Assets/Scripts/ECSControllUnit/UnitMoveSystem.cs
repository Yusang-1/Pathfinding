using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.ECSControllUnit
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct UnitMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            Entity commandEntity;
            // 이동 요청이 있는지 확인
            if (SystemAPI.TryGetSingleton(out UnitMoveCommandComponent moveCommand))
            {
                if (SystemAPI.TryGetSingletonEntity<UnitMoveCommandComponent>(out commandEntity)) { }
                else return;
            }
            else
            {
                return;
            }

            foreach (var (unitComponent, transfrom) in
                SystemAPI.Query<ECSUnitComponent, RefRW<LocalTransform>>().WithAll<SelectedUnitTag>())
            {

                // 테스트, 즉시 이동
                transfrom.ValueRW.Position = moveCommand.Destination;
            }

            state.EntityManager.DestroyEntity(commandEntity);
        }
    }

    public struct UnitMoveCommandComponent : IComponentData
    {
        public float3 Destination;
    }
}

