using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public class MouseTargetAuthoring : MonoBehaviour
    {
        class Baker : Baker<MouseTargetAuthoring>
        {
            public override void Bake(MouseTargetAuthoring authoring)
            {
                var mouseTargetData = new MouseTargetData();
                AddComponent(GetEntity(TransformUsageFlags.None), mouseTargetData);
            }
        }
        
    }
}
