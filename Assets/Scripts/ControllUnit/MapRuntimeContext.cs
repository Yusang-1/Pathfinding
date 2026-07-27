namespace Assets.Scripts.ControllUnit
{
    public class MapRuntimeContext
    {
        public MapRuntimeContext(Pathfinder pathfinder)
        {
            Pathfinder = pathfinder;
        }

        public NodeList NodeList { get; private set; } = new();
        public SpatialHash SpatialHash { get; private set; } = new();
        public Pathfinder Pathfinder { get; private set; }
    }
}
