using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.ControllUnit
{
    public class MapRuntimeContext
    {
        public MapRuntimeContext(PathfinderControllUnit pathfinder, NodeData nodeData)
        {
            Pathfinder = pathfinder;
            NodeList = new NodeList(nodeData);
        }

        public NodeList NodeList { get; private set; }
        public SpatialHash SpatialHash { get; private set; } = new();
        public PathfinderControllUnit Pathfinder { get; private set; }
    }
}
