using UnityEngine;
using Unity.Entities;

public struct ECSCube : IComponentData
{
    public float Speed;
}

public class ECSCubeAuthoring : MonoBehaviour
{
    public float Speed;

    class Baker : Baker<ECSCubeAuthoring>
    {
        public override void Bake(ECSCubeAuthoring authoring)
        {
            // TransformUsageFlags : 게임 오브젝트의 변환(Transform) 정보를 엔티티에 어떤 방식으로 부여할지 결정하는 설정값
            var data = new ECSCube() { Speed = authoring.Speed };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), data);
        }
    }
}
