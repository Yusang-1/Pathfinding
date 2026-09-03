using UnityEngine;
using Unity.Entities;

namespace Assets.Scripts.ECSControllUnit
{
    public class UnitPrefabMaker : MonoBehaviour
    {
        public UnitAuthoring unitPrefab;
        public UnitAuthoring unitLargePrefab;
        public UnitBottomAuthoring unitBottomAuthoring;

        public class Baker : Baker<UnitPrefabMaker>
        {
            public override void Bake(UnitPrefabMaker authoring)
            {
                Entity spawnerDataEntity = CreateAdditionalEntity(TransformUsageFlags.None, false);

                Entity prefabEntity = GetEntity(authoring.unitPrefab, TransformUsageFlags.Dynamic);
                Entity prefabEntityLarge = GetEntity(authoring.unitLargePrefab, TransformUsageFlags.Dynamic);

                // var UnitPrefabComponent = new UnitPrefabComponent();
                // AddComponent(spawnerDataEntity, UnitPrefabComponent);

                var entityBySizeBuffer = AddBuffer<UnitBySizeDynamicBuffer>(spawnerDataEntity);

                GetUnitBySizeBuffer(entityBySizeBuffer, UnitSize.small, prefabEntity);
                GetUnitBySizeBuffer(entityBySizeBuffer, UnitSize.large, prefabEntityLarge);

                Entity bottomEntity = GetEntity(authoring.unitBottomAuthoring, TransformUsageFlags.Dynamic);
                AddComponent(spawnerDataEntity, new UnitBottomContainer { Prefab = bottomEntity });
            }

            private void GetUnitBySizeBuffer(DynamicBuffer<UnitBySizeDynamicBuffer> entityBySizeBuffer, UnitSize unitSize, Entity entity)
            {
                entityBySizeBuffer.Add(
                    new UnitBySizeDynamicBuffer
                    {
                        Key = unitSize,
                        Value = entity
                    }
                );
            }
        }
    }

    // public struct UnitPrefabComponent : IComponentData { }

    public struct UnitBySizeDynamicBuffer : IBufferElementData
    {
        public UnitSize Key;
        public Entity Value;
    }

    public struct UnitBottomContainer : IComponentData
    {
        public Entity Prefab;
    }
}

