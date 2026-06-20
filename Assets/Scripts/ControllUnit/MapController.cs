using UnityEngine;
using Assets.Scripts.ControllUnit.UI;

namespace Assets.Scripts.ControllUnit
{
    public class MapController : MonoBehaviour
    {
        private NodeList nodeList;
        private MapGenerator mapGenerator;
        private MapdataJsonConverter mapdataJsonConverter;
        private readonly SelectableController selectableController = new();
        private readonly SpatialHash spatialHash = new();

        [SerializeField] private InputManager inputManager;
        [SerializeField] private Pathfinder pathfinder;
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private ControllUnitUIRoot uiRoot;
        [SerializeField] private Node nodePrefab;
        [SerializeField] private NodeData nodeData;

        [Header("Values")]
        private int nodeSize;
        private int mapSize;
        private int clusterSize;

        private void Start()
        {
            nodeList = new NodeList(nodeData);
            mapdataJsonConverter = new MapdataJsonConverter();
            mapGenerator = new MapGenerator(nodePrefab, nodeList);

            nodeData.Initialize();
            unitSpawner.Initialize(spatialHash);
            inputManager.Initialize(selectableController);

            uiRoot.OnLoadMapRequested += SetMapData;
            uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnSpawnUnitRequested += unitSpawner.SpawnUnit;            
            uiRoot.OnFindSelectableUnitInDragUI += spatialHash.GetUnitsInRange;
            uiRoot.OnUnitFocused += selectableController.UnitFocusedList;

            unitSpawner.OnSelectedCallback += (selectable) => uiRoot.OnUnitSelected?.Invoke(selectable);
            unitSpawner.OnDeselectedCallback += (selectable) => uiRoot.OnUnitDeselected?.Invoke(selectable);

            inputManager.OnHoldStarted += (vec) => uiRoot.OnHoldStarted?.Invoke(vec);
            inputManager.OnHoldPreformed += (vec) => uiRoot.OnHoldPreformed?.Invoke(vec);
            inputManager.OnHoldCanceled += () => uiRoot.OnHoldCanceled?.Invoke();
            inputManager.OnControllMenu += () => uiRoot.OnManageMenu?.Invoke();
        }

        private void SetMapData(MapData mapData)
        {
            nodeSize = mapData.NodeSize;
            mapSize = mapData.MapSize;
            clusterSize = mapData.ClusterSize;

            nodeList.Initialize(nodeSize, mapSize);

            mapGenerator.GenerateMap(mapData);

            pathfinder.SetNodeAndCluster(nodeList, mapSize, clusterSize);
        }
    }
}
