using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.ECSControllUnit
{
    [UpdateAfter(typeof(UnitSpatialHashSystem))]
    public partial struct UnitSelectionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            bool hasRequest = false;
            foreach (var a in SystemAPI.Query<UnitSelectionRequest>())
            {
                hasRequest = true;
                break;
            }
            if (!hasRequest)
            {
                return;
            }

            foreach (var (request, entity) in SystemAPI.Query<UnitSelectionRequest>().WithEntityAccess())
            {

            }
        }        
    }

    public struct SelectedUnitTag : IComponentData { }

    public struct UnitSelectionRequest : IComponentData
    {
        public float3 WorldPosition;

        /// <summary> Shift Click 여부 </summary>
        public bool IsAdditive;
    }

    public struct SelectableUnitTag : IComponentData { }
}
