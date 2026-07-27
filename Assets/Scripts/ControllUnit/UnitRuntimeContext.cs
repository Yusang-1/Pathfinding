namespace Assets.Scripts.ControllUnit
{    
    public class UnitRuntimeContext
    {
        public UnitRuntimeContext(Pathfinder pathfinder, SpatialHash spatialHash)
        {
            Pathfinder = pathfinder;
            SpatialHash = spatialHash;
        }

        public Pathfinder Pathfinder { get; private set; }
        public SteeringBehavior SteeringBehavior { get; private set; } = new();
        public SpatialHash SpatialHash { get; private set; }
    }
}
