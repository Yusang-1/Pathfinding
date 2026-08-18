namespace Assets.Scripts.ControllUnit
{
    public class MapRuntimeContext
    {
        public MapRuntimeContext(Pathfinder pathfinder, NodeData nodeData)
        {
            Pathfinder = pathfinder;
            NodeList = new NodeList(nodeData);
        }

        public NodeList NodeList { get; private set; }
        public SpatialHash SpatialHash { get; private set; } = new();
        public Pathfinder Pathfinder { get; private set; }
    }
}
