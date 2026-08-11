using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public class ECSCrowdUnitSpawnerAuthoring : MonoBehaviour
    {
        public CrowdUnitAuthoring CrowdUnitPrefab;
        
        public class Baker : Baker<ECSCrowdUnitSpawnerAuthoring>
        {
            public override void Bake(ECSCrowdUnitSpawnerAuthoring authoring)
            {
                // spawn위한 component를 가진 entity 생성
                Entity SpawnerDataEntity = CreateAdditionalEntity(TransformUsageFlags.None, false);
                Entity prefabEntity = GetEntity(authoring.CrowdUnitPrefab, TransformUsageFlags.None);
                
                var unitSpawnerData = new ECSCrowdUnitSpawnerData()
                {
                    EntityPrefab = prefabEntity
                };
                AddComponent(SpawnerDataEntity, unitSpawnerData);
            }
        }
    }
}
