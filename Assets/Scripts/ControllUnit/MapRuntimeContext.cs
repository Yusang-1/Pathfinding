using Assets.Scripts.CreateMap;
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
        
        public const int NODE_SIZE = 1;
        public const int CLUSTER_SIZE = 10;
        
        public NodeList NodeList { get; private set; }
        public SpatialHash SpatialHash { get; private set; } = new();
        public PathfinderControllUnit Pathfinder { get; private set; }
        public LoadedMapData LoadedMapData { get; private set; } = new();
    }
}
