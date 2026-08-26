using Unity.Entities;

namespace Assets.Scripts.ECSControllUnit
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct UnitMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            
        }
    }
}

