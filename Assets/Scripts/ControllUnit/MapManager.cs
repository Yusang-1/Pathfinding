using UnityEngine;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.CreateMap;

namespace Assets.Scripts.ControllUnit
{
    public class MapManager : MonoBehaviour // controllUnit 씬의 manager
    {
        private MapGenerator mapGenerator;
        private MapBootStrapper mapBootStrapper;
        private MapRuntimeContext mapRuntimeContext;
        
        private readonly LoadedMapData loadedMapData = new();

        [SerializeField] private PathfinderControllUnit pathfinder;
        [SerializeField] private Node nodePrefab;
        [SerializeField] private UnitsSO unitsSO;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private ControllUnitUIRoot uiRoot;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private UnitSpawner unitSpawner;

        private void Awake()
        {
            mapRuntimeContext = new MapRuntimeContext(pathfinder, nodeData);
            mapGenerator = new MapGenerator(nodePrefab, mapRuntimeContext.NodeList, unitSpawner);
            mapBootStrapper = new MapBootStrapper(uiRoot, inputManager, unitSpawner, InitializeMapRuntime, mapRuntimeContext);
        }

        private void OnEnable()
        {
            mapBootStrapper.BindEvents();
        }

        private void Start()
        {
            mapBootStrapper.Initialize(nodeData, unitsSO, mapRuntimeContext.Pathfinder);
        }

        private void OnDisable()
        {
            mapBootStrapper.UnbindEvents();
        }

        private void InitializeMapRuntime(int mapCode)
        {
            if (!loadedMapData.TryGetMapData(mapCode, out MapData mapData))
            {
                return;
            }

            int mapSize = mapData.InfoData.MapSize;
            mapRuntimeContext.NodeList.Initialize(MapRuntimeContext.NODE_SIZE, mapSize);

            mapGenerator.GenerateMap(mapSize, mapData.TerrainData);

            mapRuntimeContext.Pathfinder.SetNodeAndCluster(mapRuntimeContext.NodeList, mapSize, MapRuntimeContext.CLUSTER_SIZE, unitsSO.UnitRadius);
        }
    }
}
