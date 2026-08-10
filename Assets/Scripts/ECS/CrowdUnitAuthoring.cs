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
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new CrowdUnitData()
                    {
                        Speed = authoring.Speed,
                        HasDestination = authoring.HasDestination,
                        Destination = authoring.Destination,
                        Velocity = authoring.Velocity
                    }
                );
                AddComponent(entity,
                    new SteeringBehaviourData()
                    {
                        SeekWeight = authoring.SteeringWeightingData.WalkConfig.SeekWeight,
                        SeparationWeight = authoring.SteeringWeightingData.WalkConfig.SeparationWeight,
                        AlignmentWeight = authoring.SteeringWeightingData.WalkConfig.AlignmentWeight,
                        CohesionWeight = authoring.SteeringWeightingData.WalkConfig.CohesionWeight
                    }
                );
                AddComponent(entity, new MoveUnitTag());
                
                AddBuffer<NearbyEntityElement>(entity);
            }
        }
    }
}

