using UnityEngine;
using Assets.Scripts.CreateMap;

public class PathManager : MonoBehaviour // comparePathfinding 씬의 manager
{
    private NodeList nodeList;
    private readonly PathfinderComparePathfinding pathfinder = new();
    private PathManagerBootStrapper pathManagerBootStrapper;
    private MapGenerator mapGenerator;
    private LoadedMapData loadedMapData;

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
        pathManagerBootStrapper = new PathManagerBootStrapper(nodeList, uiRoot, inputManager, pathfinder, SetMapData, nodeData);
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
        if(!loadedMapData.TryGetMapData(mapCode, out MapData mapData))
        {
            return;
        }
        
        int nodeSize = Assets.Scripts.ControllUnit.MapRuntimeContext.NODE_SIZE;
        int mapSize = mapData.InfoData.MapSize;
        int clusterSize = Assets.Scripts.ControllUnit.MapRuntimeContext.CLUSTER_SIZE;

        nodeList.Initialize(nodeSize, mapSize);
                
        mapGenerator.GenerateMap(mapSize, mapData.TerrainData);
        clusterShower.Initialize(mapSize / clusterSize, clusterSize, nodeSize);

        pathfinder.SetNodeAndCluster(mapData, unitsSO.UnitRadius);
    }
}
