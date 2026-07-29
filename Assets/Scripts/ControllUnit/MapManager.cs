using UnityEngine;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class MapManager : MonoBehaviour
    {
        private MapGenerator mapGenerator;
        private MapBootStrapper mapBootStrapper;
        private MapRuntimeContext mapRuntimeContext;

        [SerializeField] private Pathfinder pathfinder;
        [SerializeField] private Node nodePrefab;
        [SerializeField] private UnitsSO unitsSO;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private ControllUnitUIRoot uiRoot;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private UnitSpawner unitSpawner;

        private void OnEnable()
        {
            mapBootStrapper = new MapBootStrapper(uiRoot, inputManager, unitSpawner);
            mapBootStrapper.BindEvents(InitializeMapRuntime, mapRuntimeContext);
        }
        
        private void Start()
        {
            mapRuntimeContext = new MapRuntimeContext(pathfinder);
            mapGenerator = new MapGenerator(nodePrefab, mapRuntimeContext.NodeList);
                        
            mapBootStrapper.Initialize(nodeData, mapRuntimeContext, unitsSO, mapRuntimeContext.Pathfinder);            
        }

        private void OnDisable()
        {
            mapBootStrapper.UnbindEvents(InitializeMapRuntime, mapRuntimeContext);
        }

        private void InitializeMapRuntime(MapData mapData)
        {
            mapRuntimeContext.NodeList.Initialize(mapData, nodeData);

            mapGenerator.GenerateMap(mapData);

            mapRuntimeContext.Pathfinder.SetNodeAndCluster(mapRuntimeContext.NodeList, mapData, unitsSO.UnitRadius);
        }
    }
}
