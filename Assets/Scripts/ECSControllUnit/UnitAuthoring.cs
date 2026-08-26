using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.ECSControllUnit
{
    public class UnitAuthoring : MonoBehaviour
    {
        [SerializeField] private string unitName;
        [SerializeField] private float radius;

        [SerializeField] private float moveSpeed;

        public class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new ECSUnitComponent
                {
                    Name = authoring.unitName,
                    Radius = authoring.radius
                });
                AddComponent(entity, new MovableComponent
                {
                    MoveSpeed = authoring.moveSpeed
                });
                AddComponent(entity, new SelectableUnitTag());
                AddComponent(entity, new SpatialHashCell());

                AddComponent(entity, new Prefab());
                AddComponent(entity, new Disabled());
            }
        }
    }

    public struct ECSUnitComponent : IComponentData
    {
        public FixedString32Bytes Name;
        public float Radius;

    }

    public struct MovableComponent : IComponentData
    {
        public float MoveSpeed;
        public float3 Direction;
        public float3 Velocity;
    }
}
