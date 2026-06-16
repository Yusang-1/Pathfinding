using UnityEngine;
using Assets.Scripts.ControllUnit.UI;

namespace Assets.Scripts.ControllUnit
{
    public class MapController : MonoBehaviour
    {
        private NodeList nodeList;
        private MapGenerator mapGenerator;
        private MapdataJsonConverter mapdataJsonConverter;

        [SerializeField] private Pathfinder pathfinder;
        [SerializeField] private ControllUnitUIRoot uiRoot;
        [SerializeField] private Node nodePrefab;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private UnitSpawner unitSpawner;

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

            uiRoot.OnLoadMapRequested += SetMapData;
            uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;

            uiRoot.OnSpawnUnitRequested += unitSpawner.SpawnUnit;
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
