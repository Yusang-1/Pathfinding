using UnityEngine;
using Unity.Entities;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.ECSControllUnit
{
    public class UnitBottomAuthoring : MonoBehaviour
    {
        public class Baker : Baker<UnitBottomAuthoring>
        {
            public override void Bake(UnitBottomAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Prefab());
                AddComponent(entity, new Disabled());
            }
        }
    }
}
