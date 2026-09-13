using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Assets.Scripts.ControllUnit.SO;
using Assets.Scripts.ECS.UnitMovement;

namespace Assets.Scripts.ECSControllUnit
{
    public class UnitAuthoring : MonoBehaviour
    {
        [SerializeField] private UnitSO unitData;
        [SerializeField] private SteeringWeightingSO steeringWeightingData;

        [SerializeField] private float unitRadius;

        public UnitSO UnitData => unitData;

        public class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new ECSUnitComponent
                {
                    UnitName = authoring.unitData.UnitName,
                    Radius = authoring.unitRadius
                    // IconName = authoring.unitData.UnitIcon.name
                });
                AddComponent(entity, new MovableComponent
                {
                    MoveSpeed = authoring.unitData.MoveSpeed,
                    ArriveDistance = 0.15f
                });
                AddComponent(entity, new UnitMoveState());
                AddComponent(entity, new SelectableUnitTag());
                AddComponent(entity, new SpatialHashCell());

                var steeringConfig = authoring.steeringWeightingData != null
                    ? authoring.steeringWeightingData.WalkConfig
                    : default;
                AddComponent(entity, new SteeringBehaviourData
                {
                    SeekWeight = steeringConfig.SeekWeight,
                    SeparationWeight = steeringConfig.SeparationWeight,
                    AlignmentWeight = steeringConfig.AlignmentWeight,
                    CohesionWeight = steeringConfig.CohesionWeight
                });

                AddComponent(entity, new Prefab());
                AddComponent(entity, new Disabled());

                AddBuffer<HighLevelClusterPath>(entity);
                AddBuffer<HighLevelWaypoint>(entity);
                AddBuffer<LowLevelWaypoint>(entity);
                AddBuffer<NearbyEntityElement>(entity);
            }
        }
    }

    public struct ECSUnitComponent : IComponentData
    {
        public FixedString32Bytes UnitName;
        public FixedString64Bytes IconName;
        public float Radius;
        public Entity BottomCircleSelected;
        public Entity BottomCircleFocused;
    }

    public struct MovableComponent : IComponentData
    {
        public float MoveSpeed;
        public float3 Direction;
        public float3 Velocity;

        public float ArriveDistance;
    }
}
