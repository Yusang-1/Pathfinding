using UnityEngine;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.CreateMap;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSMapManager : MonoBehaviour
    {
        [SerializeField] private ControllUnitUIRoot uiRoot;
        [SerializeField] private ECSInputManager inputManager;
        [SerializeField] private ECSUnitSpawner unitSpawner;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private PathfinderControllUnit pathfinder;
        [SerializeField] private UnitsSO unitsSO;
        [SerializeField] private Node nodePrefab;
        [SerializeField] private ECSPathfindingBridge pathfindingBridge;

        private MapRuntimeContext mapRuntimeContext;
        private MapGenerator mapGenerator;
        private readonly ECSSelectableController selectableController = new();                

        private ECSMapManagerBootStrapper bootStrapper;

        private void Awake()
        {
            mapRuntimeContext = new MapRuntimeContext(pathfinder, nodeData);
            mapGenerator = new MapGenerator(nodePrefab, mapRuntimeContext.NodeList, unitSpawner);
            bootStrapper = new ECSMapManagerBootStrapper(uiRoot, inputManager, unitSpawner, InitializeMapRuntime,
                mapRuntimeContext, pathfindingBridge, selectableController
            );            
        }

        private void OnEnable()
        {
            bootStrapper.BindEvents();
        }

        private void Start()
        {
            bootStrapper.Initialize(nodeData, unitsSO, mapRuntimeContext.Pathfinder);            
        }

        private void Update()
        {
            selectableController.SelectedUpdate();
        }
        
        private void OnDisable()
        {
            bootStrapper.UnbindEvents();
        }

        private void InitializeMapRuntime(int mapCode)
        {
            if (!mapRuntimeContext.LoadedMapData.TryGetMapData(mapCode, out MapData mapData))
            {
                return;
            }
            
            int mapSize = mapData.InfoData.MapSize;
            mapRuntimeContext.NodeList.Initialize(MapRuntimeContext.NODE_SIZE, mapSize);

            mapGenerator.GenerateMap(mapSize, mapData.TerrainData);
            mapGenerator.SetUnit(mapData.UnitData);

            pathfindingBridge.SetNodeAndCluster(mapRuntimeContext.NodeList, mapSize, unitsSO.UnitRadius);
        }
    }
}
