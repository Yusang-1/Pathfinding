using Assets.Scripts.ECS.UnitMovement;
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
            foreach (var (moveState, transform, movableComponent, unitComponent, steeringData, neighborsBuffer, entity)
                in SystemAPI.Query<RefRW<UnitMoveState>, RefRW<LocalTransform>, RefRW<MovableComponent>,
                    RefRO<ECSUnitComponent>, RefRO<SteeringBehaviourData>, DynamicBuffer<NearbyEntityElement>>().WithEntityAccess())
            {
                if (!moveState.ValueRO.IsMoving) continue;

                EntityManager entityManager = state.EntityManager;

                // 목적지 받음
                var waypointBuffer = entityManager.GetBuffer<LowLevelWaypoint>(entity);

                float3 destination = waypointBuffer[moveState.ValueRO.LowLevelPathIndex].Position;

                // 받은 목적지가 버퍼의 마지막 요소였다면 lazy refine 요청
                if (moveState.ValueRO.LowLevelPathIndex == waypointBuffer.Length - 1 && !moveState.ValueRW.IsNeedLazyRefine)
                {
                    moveState.ValueRW.IsNeedLazyRefine = true;
                }

                // steering
                float3 steering = CalculateSteering(transform.ValueRO.Position, movableComponent.ValueRO.Velocity,
                    destination, unitComponent.ValueRO.Radius, movableComponent.ValueRO.MoveSpeed,
                    steeringData, entity, neighborsBuffer, entityManager
                );
                
                float3 velocity = movableComponent.ValueRW.Velocity + steering * Time.deltaTime;
                velocity = math.clamp(velocity, -movableComponent.ValueRO.MoveSpeed, movableComponent.ValueRO.MoveSpeed);

                movableComponent.ValueRW.Velocity = velocity;
                transform.ValueRW.Position += velocity * Time.deltaTime;

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

                Entity bottomEntity;
                Entity bottomEntitySelected = unitComponent.ValueRO.BottomCircleSelected;
                Entity bottomEnttiyFocused = unitComponent.ValueRO.BottomCircleFocused;
                if (!state.EntityManager.HasComponent(bottomEntitySelected, typeof(Disabled)))
                {
                    bottomEntity = bottomEntitySelected;
                }
                else if (!state.EntityManager.HasComponent(bottomEnttiyFocused, typeof(Disabled)))
                {
                    bottomEntity = bottomEntitySelected;
                }
                else continue;

                if (bottomEntity != Entity.Null && state.EntityManager.HasComponent<LocalTransform>(bottomEntity))
                {
                    LocalTransform bottomTransform = state.EntityManager.GetComponentData<LocalTransform>(bottomEntity);

                    bottomTransform.Position = transform.ValueRO.Position;

                    state.EntityManager.SetComponentData(bottomEntity, bottomTransform);
                }
            }
        }


        private float3 CalculateSteering(float3 unitPosition, float3 currentVelocity, float3 destination,
            float radius, float maxSpeed, RefRO<SteeringBehaviourData> weighting, Entity unit,
            DynamicBuffer<NearbyEntityElement> nearby, EntityManager entityManager)
        {
            float distToGoal = Vector3.Distance(unitPosition, destination);
            float arrivalRadius = 0.15f;

            if (distToGoal < arrivalRadius)
            {
                return float3.zero;
            }

            var seekVector = Seek(unitPosition, destination, maxSpeed, currentVelocity);
            seekVector *= weighting.ValueRO.SeekWeight;

            if (nearby.IsEmpty)
            {
                return seekVector;
            }

            var separationVector = Separation(unitPosition, radius, unit, nearby, entityManager);
            float separationScale = Mathf.Clamp01((distToGoal - arrivalRadius) / 2f);
            separationVector *= weighting.ValueRO.SeparationWeight * separationScale;

            var cohesionVector = Cohesion(unitPosition, unit, nearby, maxSpeed, entityManager);
            cohesionVector *= weighting.ValueRO.CohesionWeight;

            var alignmentVector = Alignment(unit, nearby, currentVelocity, entityManager);
            alignmentVector *= weighting.ValueRO.AlignmentWeight;

            return seekVector + separationVector + cohesionVector + alignmentVector;
        }

        private float3 Seek(float3 unitPosition, float3 target, float maxSpeed, float3 velocity)
        {
            float3 desired = ((Vector3)(target - unitPosition)).normalized * maxSpeed;
            return desired - velocity;
        }

        private float3 Separation(float3 unitPosition, float radius, Entity unit,
            DynamicBuffer<NearbyEntityElement> nearby, EntityManager entityManager)
        {
            float3 steeringForce = float3.zero;

            foreach (var value in nearby)
            {
                Entity other = value.Value;

                if (other == unit) continue;

                ECSUnitComponent otherData = entityManager.GetComponentData<ECSUnitComponent>(other);
                float separationRadius = (otherData.Radius + radius) * 1.2f;

                LocalTransform otherTransform = entityManager.GetComponentData<LocalTransform>(other);
                float distance = Vector3.Distance(unitPosition, otherTransform.Position);

                if (distance < separationRadius && distance > 0.01f)
                {
                    float3 diff = ((Vector3)(unitPosition - otherTransform.Position)).normalized;
                    diff /= distance;
                    steeringForce += diff;
                }
            }

            return steeringForce;
        }

        private float3 Cohesion(float3 unitPosition, Entity unit, DynamicBuffer<NearbyEntityElement> nearby,
            float maxSpeed, EntityManager entityManager)
        {
            float3 centerOfMass = float3.zero;

            foreach (var value in nearby)
            {
                Entity other = value.Value;

                if (other == unit) continue;

                LocalTransform otherTransform = entityManager.GetComponentData<LocalTransform>(other);
                centerOfMass += otherTransform.Position;
            }

            centerOfMass /= nearby.Length;
            MovableComponent unitData = entityManager.GetComponentData<MovableComponent>(unit);
            return Seek(unitPosition, centerOfMass, maxSpeed, unitData.Velocity);
        }

        private float3 Alignment(Entity unit, DynamicBuffer<NearbyEntityElement> nearby, float3 currentVelocity, EntityManager entityManager)
        {
            float3 averageVelocity = float3.zero;

            foreach (var value in nearby)
            {
                Entity other = value.Value;

                if (other == unit) continue;

                MovableComponent otherData = entityManager.GetComponentData<MovableComponent>(other);
                averageVelocity += otherData.Velocity;
            }
            return averageVelocity / nearby.Length - currentVelocity;
        }
    }
}

public struct UnitMoveCommandComponent : IComponentData
{
    public float3 Destination;
    public bool IsAdditive;
}
