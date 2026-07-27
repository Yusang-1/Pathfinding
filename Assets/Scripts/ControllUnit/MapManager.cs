using UnityEngine;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class MapManager : MonoBehaviour
    {
        private MapGenerator mapGenerator;
        private MapBootStrapper mapBootStrapper;
        private readonly MapRuntimeContext mapRuntimeContext = new();

        [SerializeField] private Pathfinder pathfinder;
        [SerializeField] private Node nodePrefab;
        [SerializeField] private UnitsSO unitsSO;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private ControllUnitUIRoot uiRoot;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private UnitSpawner unitSpawner;        


        private void Start()
        {
            mapGenerator = new MapGenerator(nodePrefab, mapRuntimeContext.NodeList);
            mapBootStrapper = new MapBootStrapper(uiRoot, inputManager, unitSpawner);
            
            mapBootStrapper.Initialize(nodeData, mapRuntimeContext, unitsSO);
            mapBootStrapper.BindEvents(InitializeMapRuntime, mapRuntimeContext);
        }

        private void OnDestroy()
        {
            mapBootStrapper.ResetBootStrapper(InitializeMapRuntime, mapRuntimeContext);
        }

        private void InitializeMapRuntime(MapData mapData)
        {
            mapRuntimeContext.NodeList.Initialize(mapData, nodeData);

            mapGenerator.GenerateMap(mapData);

            pathfinder.SetNodeAndCluster(mapRuntimeContext.NodeList, mapData, unitsSO.UnitRadius);
        }
    }
}
