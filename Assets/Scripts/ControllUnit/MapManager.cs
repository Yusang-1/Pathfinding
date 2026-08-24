using UnityEngine;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;
using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.ControllUnit
{
    public class MapManager : MonoBehaviour
    {
        private MapGenerator mapGenerator;
        private MapBootStrapper mapBootStrapper;
        private MapRuntimeContext mapRuntimeContext;

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
            mapGenerator = new MapGenerator(nodePrefab, mapRuntimeContext.NodeList);
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

        private void InitializeMapRuntime(MapData mapData)
        {
            mapRuntimeContext.NodeList.Initialize(mapData.NodeSize, mapData.MapSize);

            mapGenerator.GenerateMap(mapData);

            mapRuntimeContext.Pathfinder.SetNodeAndCluster(mapRuntimeContext.NodeList, mapData, unitsSO.UnitRadius);
        }
    }
}
