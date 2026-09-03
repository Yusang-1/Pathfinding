using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ECSControllUnit
{
    public class UnitAuthoring : MonoBehaviour
    {
        [SerializeField] private UnitSO unitData;
        [SerializeField] private float unitRadius;

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

                AddComponent(entity, new Prefab());
                AddComponent(entity, new Disabled());

                AddBuffer<HighLevelClusterPath>(entity);
                AddBuffer<HighLevelWaypoint>(entity);
                AddBuffer<LowLevelWaypoint>(entity);
            }
        }
    }

    public struct ECSUnitComponent : IComponentData
    {
        public FixedString32Bytes UnitName;
        public FixedString64Bytes IconName;        
        public float Radius;
        public Entity BottomCircle;
    }

    public struct MovableComponent : IComponentData
    {
        public float MoveSpeed;
        public float3 Direction;
        public float3 Velocity;

        public float ArriveDistance;
    }
}
