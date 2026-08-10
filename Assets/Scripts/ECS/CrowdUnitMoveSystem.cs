using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.ECS
{
    public partial struct CrowdUnitMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (agent, steeringData, transform, neighbors, entity) in SystemAPI.Query<
                RefRW<CrowdUnitData>,
                RefRO<SteeringBehaviourData>,
                RefRW<LocalTransform>,
                DynamicBuffer<NearbyEntityElement>>()
                .WithAll<MoveUnitTag>()
                .WithEntityAccess())
            {                
                if (!agent.ValueRO.HasDestination)
                {                    
                    continue;
                }
                
                float3 currentPos = transform.ValueRO.Position;
                float3 dest = agent.ValueRO.Destination;
                
                // for (int i = 0; i < neighbors.Length; i++)
                // {
                //     Entity other = neighbors[i].Value;
                //     // 다른 에이전트와 거리/스티어링 계산
                // }
                DynamicBuffer<NearbyEntityElement> nearby = neighbors; // spatialHash.GetUnitsInRange(thisUnit.transform.position, 1);

                float3 steering = CalculateSteering(currentPos, float3.zero/*수정*/, dest, agent.ValueRO.Radius, agent.ValueRO.Speed, steeringData, entity, nearby/*수정*/);

                float3 move = steering * deltaTime;
                move = math.clamp(move, new float3(-agent.ValueRO.Speed), new float3(agent.ValueRO.Speed));
                                
                transform.ValueRW.Position += move;
                agent.ValueRW.Velocity = move;

                if (math.distancesq(currentPos, dest) <= 0.0001f)
                {
                    agent.ValueRW.HasDestination = false;
                }
            }
        }

        private float3 CalculateSteering(float3 unitPosition, float3 currentVelocity, float3 destination, float radius, float maxSpeed, RefRO<SteeringBehaviourData> weighting, Entity unit, DynamicBuffer<NearbyEntityElement> nearby)
        {
            float distToGoal = Vector3.Distance(unitPosition, destination);
            float arrivalRadius = 0.5f;

            if (distToGoal < arrivalRadius)
            {
                return float3.zero;
            }

            var seekVector = Seek(unitPosition, destination, maxSpeed, currentVelocity);
            seekVector *= weighting.ValueRO.SeekWeight;

            if (nearby.IsEmpty || nearby.Length <= 1) return seekVector;

            var separationVector = Separation(unitPosition, radius, unit, nearby);
            float separationScale = Mathf.Clamp01((distToGoal - arrivalRadius) / 2f);
            separationVector *= weighting.ValueRO.SeparationWeight * separationScale;

            var cohesionVector = Cohesion(unitPosition, unit, nearby, maxSpeed);
            cohesionVector *= weighting.ValueRO.CohesionWeight;

            var alignmentVector = Alignment(unit, nearby);
            alignmentVector *= weighting.ValueRO.AlignmentWeight;

            return seekVector + separationVector + cohesionVector + alignmentVector;
        }

        private float3 Seek(float3 unitPosition, float3 target, float maxSpeed, float3 velocity)
        {
            float3 desired = ((Vector3)(target - unitPosition)).normalized * maxSpeed;
            return desired - velocity;
        }

        private float3 Separation(float3 unitPosition, float radius, Entity unit, DynamicBuffer<NearbyEntityElement> nearby)
        {
            float3 steeringForce = float3.zero;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var value in nearby)
            {
                Entity other = value.Value;

                if (other == unit) continue;

                CrowdUnitData otherData = entityManager.GetComponentData<CrowdUnitData>(other);
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

        private float3 Cohesion(float3 unitPosition, Entity unit, DynamicBuffer<NearbyEntityElement> nearby, float maxSpeed)
        {
            float3 centerOfMass = float3.zero;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var value in nearby)
            {
                Entity other = value.Value;

                if (other == unit) continue;

                LocalTransform otherTransform = entityManager.GetComponentData<LocalTransform>(other);
                centerOfMass += otherTransform.Position;
            }

            centerOfMass /= nearby.Length;
            CrowdUnitData unitData = entityManager.GetComponentData<CrowdUnitData>(unit);
            return Seek(unitPosition, centerOfMass, maxSpeed, unitData.Velocity);
        }

        private float3 Alignment(Entity unit, DynamicBuffer<NearbyEntityElement> nearby)
        {
            float3 averageVelocity = float3.zero;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var value in nearby)
            {
                Entity other = value.Value;

                if (other == unit) continue;

                CrowdUnitData otherData = entityManager.GetComponentData<CrowdUnitData>(other);
                averageVelocity += otherData.Velocity;
            }
            return averageVelocity /= nearby.Length;
        }
    }
}
