using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public struct GeneratePrefabStruct : IComponentData
    {
        public Entity prefab;
    }
}
