using UnityEngine;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit.SO;
using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSMapManager : MonoBehaviour
    {
        [SerializeField] private ControllUnitUIRoot uiRoot;
        [SerializeField] private Assets.Scripts.ControllUnit.InputManager inputManager;
        [SerializeField] private ECSUnitSpawner unitSpawner;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private PathfinderControllUnit pathfinder;
        [SerializeField] private UnitsSO unitsSO;
        [SerializeField] private Node nodePrefab;
        
        private Assets.Scripts.ControllUnit.MapRuntimeContext mapRuntimeContext;
        private MapGenerator mapGenerator;
        
        private ECSMapManagerBootStrapper bootStrapper;

        private void Awake()
        {
            mapRuntimeContext = new Assets.Scripts.ControllUnit.MapRuntimeContext(pathfinder, nodeData);
            mapGenerator = new MapGenerator(nodePrefab, mapRuntimeContext.NodeList);
            bootStrapper = new ECSMapManagerBootStrapper(uiRoot, inputManager, unitSpawner, InitializeMapRuntime, mapRuntimeContext);
        }
        
        private void OnEnable()
        {            
            bootStrapper.BindEvents();
        }

        private void Start()
        {
            bootStrapper.Initialize(nodeData, unitsSO, mapRuntimeContext.Pathfinder);
        }

        private void OnDisable()
        {
            bootStrapper.UnbindEvents();
        }
        
        private void InitializeMapRuntime(MapData mapData)
        {
            mapRuntimeContext.NodeList.Initialize(mapData.NodeSize, mapData.MapSize);

            mapGenerator.GenerateMap(mapData);

            mapRuntimeContext.Pathfinder.SetNodeAndCluster(mapRuntimeContext.NodeList, mapData, unitsSO.UnitRadius);
        }
    }
}
