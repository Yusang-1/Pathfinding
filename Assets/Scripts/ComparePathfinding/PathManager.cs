using UnityEngine;
using Assets.Scripts.CreateMap;
using Assets.Scripts.ControllUnit;

public class PathManager : MonoBehaviour // comparePathfinding 씬의 manager
{
    private NodeList nodeList;
    private readonly PathfinderComparePathfinding pathfinder = new();
    private PathManagerBootStrapper pathManagerBootStrapper;
    private MapGenerator mapGenerator;
    private MapRuntimeContext mapRuntimeContext;

    [SerializeField] private UIRoot uiRoot;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private NodeData nodeData;
    [SerializeField] private ClusterShower clusterShower;
    [SerializeField] private LineDrawer lineDrawer;
    [SerializeField] private Assets.Scripts.ControllUnit.SO.UnitsSO unitsSO;
    [SerializeField] private UnitUncontrollable unit;
    [SerializeField] private AbstractSpawner unitSpawner;

    private void Awake()
    {
        nodeList = new NodeList(nodeData);
        pathfinder.Initialize(nodeList, clusterShower, lineDrawer, unit);
        unitsSO.Initialize();
        mapGenerator = new MapGenerator(nodePrefab, nodeList, unitSpawner);
        mapRuntimeContext = new MapRuntimeContext(null, nodeData);
        pathManagerBootStrapper = new PathManagerBootStrapper(nodeList, uiRoot, inputManager, pathfinder, SetMapData, mapRuntimeContext);
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

    private void SetMapData(int mapCode)
    {
        if(!mapRuntimeContext.LoadedMapData.TryGetMapData(mapCode, out MapData mapData))
        {
            return;
        }
        
        int nodeSize = MapRuntimeContext.NODE_SIZE;
        int mapSize = mapData.InfoData.MapSize;
        int clusterSize = MapRuntimeContext.CLUSTER_SIZE;

        nodeList.Initialize(nodeSize, mapSize);
                
        mapGenerator.GenerateMap(mapSize, mapData.TerrainData);
        clusterShower.Initialize(mapSize / clusterSize, clusterSize, nodeSize);

        pathfinder.SetNodeAndCluster(mapData, unitsSO.UnitRadius);
    }
}
