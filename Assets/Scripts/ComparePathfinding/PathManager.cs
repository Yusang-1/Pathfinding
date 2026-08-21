using UnityEngine;

public class PathManager : MonoBehaviour
{
    private NodeList nodeList;    
    private readonly PathfinderComparePathfinding pathfinder = new();
    private PathManagerBootStrapper pathManagerBootStrapper;

    [SerializeField] private UIRoot uiRoot;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private NodeData nodeData;
    [SerializeField] private ClusterShower clusterShower;
    [SerializeField] private LineDrawer lineDrawer;
    [SerializeField] private Assets.Scripts.ControllUnit.SO.UnitsSO unitsSO;
    [SerializeField] private UnitUncontrollable unit;

    [Header("Values")]
    private int nodeSize;
    private int clusterSize;
    private int mapSize;
    
    private void Awake()
    {
        nodeList = new NodeList(nodeData);
        pathfinder.Initialize(nodeList, clusterShower, lineDrawer, unit);         
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
        nodeSize = mapData.NodeSize;
        mapSize = mapData.MapSize;
        clusterSize = mapData.ClusterSize;

        nodeList.Initialize(nodeSize, mapSize);
        clusterShower.Initialize(mapSize / clusterSize, clusterSize, nodeSize);

        pathfinder.SetNodeAndCluster(mapData, unitsSO.UnitRadius);
    }
}
