using UnityEngine;

public class PathManager : MonoBehaviour
{
    private NodeList nodeList;
    private readonly PathfinderComparePathfinding pathfinder = new();
    private PathManagerBootStrapper pathManagerBootStrapper;
    private MapGenerator mapGenerator;

    [SerializeField] private UIRoot uiRoot;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private NodeData nodeData;
    [SerializeField] private ClusterShower clusterShower;
    [SerializeField] private LineDrawer lineDrawer;
    [SerializeField] private Assets.Scripts.ControllUnit.SO.UnitsSO unitsSO;
    [SerializeField] private UnitUncontrollable unit;

    private void Awake()
    {
        nodeList = new NodeList(nodeData);
        pathfinder.Initialize(nodeList, clusterShower, lineDrawer, unit);
        unitsSO.Initialize();
        mapGenerator = new MapGenerator(nodePrefab, nodeList);
        pathManagerBootStrapper = new PathManagerBootStrapper(nodePrefab, nodeList, uiRoot, inputManager, pathfinder, SetMapData);
    }

    private void OnEnable()
    {
        pathManagerBootStrapper.BindEvents();
    }

    private void Start()
    {
        nodeData.Initialize();
        uiRoot.Initialize();
    }

    private void OnDisable()
    {
        pathManagerBootStrapper.UnbindEvents();
    }

    private void SetMapData(MapData mapData)
    {
        int nodeSize = mapData.NodeSize;
        int mapSize = mapData.MapSize;
        int clusterSize = mapData.ClusterSize;

        nodeList.Initialize(nodeSize, mapSize);
                
        mapGenerator.GenerateMap(mapData);
        clusterShower.Initialize(mapSize / clusterSize, clusterSize, nodeSize);

        pathfinder.SetNodeAndCluster(mapData, unitsSO.UnitRadius);
    }
}
