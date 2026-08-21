using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.ControllUnit
{    
    public class UnitRuntimeContext
    {
        public UnitRuntimeContext(PathfinderControllUnit pathfinder, SpatialHash spatialHash)
        {
            Pathfinder = pathfinder;
            SpatialHash = spatialHash;
        }

        public PathfinderControllUnit Pathfinder { get; private set; }
        public SteeringBehavior SteeringBehavior { get; private set; } = new();
        public SpatialHash SpatialHash { get; private set; }
    }
}
