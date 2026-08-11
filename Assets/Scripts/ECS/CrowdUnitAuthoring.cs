using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.ECS
{
    public class CrowdUnitAuthoring : MonoBehaviour
    {
        public float Speed;
        public bool HasDestination;
        public float3 Destination;
        public float3 Velocity;
        public SteeringWeightingSO SteeringWeightingData;

        class Baker : Baker<CrowdUnitAuthoring>
        {
            public override void Bake(CrowdUnitAuthoring authoring)
            {
                Entity prefabEntity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(prefabEntity,
                    new CrowdUnitData()
                    {
                        Speed = authoring.Speed,
                        HasDestination = authoring.HasDestination,
                        Destination = authoring.Destination,
                        Velocity = authoring.Velocity
                    }
                );

                var steeringConfig = authoring.SteeringWeightingData != null
                    ? authoring.SteeringWeightingData.WalkConfig
                    : default;
                AddComponent(prefabEntity, new SteeringBehaviourData
                {
                    SeekWeight = steeringConfig.SeekWeight,
                    SeparationWeight = steeringConfig.SeparationWeight,
                    AlignmentWeight = steeringConfig.AlignmentWeight,
                    CohesionWeight = steeringConfig.CohesionWeight
                });

                AddComponent(prefabEntity, new MoveUnitTag());

                AddComponent(prefabEntity, new Prefab());
                AddComponent(prefabEntity, new Disabled());

                AddBuffer<NearbyEntityElement>(prefabEntity);                
            }
        }
    }
}

