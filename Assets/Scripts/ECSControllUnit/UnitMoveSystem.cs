using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECSControllUnit
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct UnitMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (moveState, transform, movableComponent, entity)
                in SystemAPI.Query<RefRW<UnitMoveState>, RefRW<LocalTransform>, RefRW<MovableComponent>>().WithEntityAccess())
            {
                if (!moveState.ValueRO.IsMoving) continue;

                // 목적지 받음
                var waypointBuffer = state.EntityManager.GetBuffer<LowLevelWaypoint>(entity);

                float3 destination = waypointBuffer[moveState.ValueRO.LowLevelPathIndex].Position;

                // 받은 목적지가 버퍼의 마지막 요소였다면 lazy refine 요청
                if (moveState.ValueRO.LowLevelPathIndex == waypointBuffer.Length - 1 && !moveState.ValueRW.IsNeedLazyRefine)
                {
                    moveState.ValueRW.IsNeedLazyRefine = true;
                }

                // 이동
                float3 direction;
                if (Equals(destination, transform.ValueRO.Position))
                {
                    direction = float3.zero;
                }
                else
                {
                    direction = math.normalize(destination - transform.ValueRO.Position);
                }
                float3 velocity = movableComponent.ValueRO.MoveSpeed * Time.deltaTime * direction;

                movableComponent.ValueRW.Direction = direction;
                movableComponent.ValueRW.Velocity = velocity;

                transform.ValueRW.Position += velocity;

                // 목적지까지의 거리가 일정 이하일 경우 다음 update에서는 다음 목적지를 받아옴
                float arrivaDistance = movableComponent.ValueRO.ArriveDistance;

                if (math.distancesq(transform.ValueRO.Position, destination) <= arrivaDistance * arrivaDistance)
                {
                    moveState.ValueRW.LowLevelPathIndex++;

                    transform.ValueRW.Position = destination;

                    // 도착지가 버퍼의 마지막 요소였다면 종료
                    if (moveState.ValueRO.LowLevelPathIndex == waypointBuffer.Length)
                    {
                        moveState.ValueRW.IsMoving = false;
                        moveState.ValueRW.IsNeedLazyRefine = false;
                    }
                }
            }
        }
    }

    public struct UnitMoveCommandComponent : IComponentData
    {
        public float3 Destination;
        public bool IsAdditive;
    }
}

