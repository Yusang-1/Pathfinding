using UnityEngine;
using Assets.Scripts.CreateMap.UI;

namespace Assets.Scripts.CreateMap
{
    public class CreateMapManager : MonoBehaviour
    {
        [SerializeField] private Node nodePrefab;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private CreateMapUIRoot uiRoot;
        [SerializeField] private InputManager inputManager;

        private NodeList nodeList;
        private MapGenerator mapGenerator;
        private MapdataJsonConverter mapdataJsonConverter;

        [SerializeField] private int nodeSize;
        private int mapSize;
        private int clusterSize;

        private void Start()
        {
            nodeData.Initialize();

            nodeList = new NodeList(nodeData);
            nodeList.OnSelected += nodeList.NodeTypeController.SetNodeType;

            mapdataJsonConverter = new MapdataJsonConverter();
            mapGenerator = new MapGenerator(nodePrefab, nodeList);

            uiRoot.OnGenerateMapRequested += CreateEmptyMap;
            uiRoot.OnTileSelectorRequested += nodeList.NodeTypeController.SetCurrentSelected;
            uiRoot.OnExportMapRequested += ExportMap;
            uiRoot.OnClearMapRequested += nodeList.NodeTypeController.NodeTypeDrawer.ResetAllNodes;
            uiRoot.OnRemoveMapRequested += nodeList.NodeTypeController.NodeTypeDrawer.ResetAllNodes;
            uiRoot.OnRemoveMapRequested += nodeList.DestroyNodes;
            uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnLoadMapRequested += LoadSavedMap;
            uiRoot.Initialize();
            
            inputManager.OnControllMenu += () => uiRoot.OnControllMenu?.Invoke();
        }

        private void CreateEmptyMap(int sizeOfMap, int sizeOfCluster)
        {
            const int defaultMapSize = 20;
            const int maxClusterSize = 10;

            if (sizeOfMap == 0)
            {
                mapSize = defaultMapSize;
            }
            else
            {
                mapSize = sizeOfMap;
            }

            if (sizeOfCluster == 0)
            {
                clusterSize = mapSize / 4;
                clusterSize = Mathf.Clamp(clusterSize, 0, maxClusterSize);
            }
            else
            {
                clusterSize = sizeOfCluster;
            }

            mapGenerator.GenerateMap(mapSize);
        }

        public void LoadSavedMap(MapData mapData)
        {
            mapSize = mapData.MapSize;
            clusterSize = mapData.ClusterSize;

            mapGenerator.GenerateMap(mapData);
        }

        public void ExportMap(string mapName)
        {
            Vector2Int[] obstacleIndexes = nodeList.NodeTypeController.NodeTypeDrawer.GetNodeInfo()[NodeType.obstacle].ToArray();

            MapData mapData = new()
            {
                MapName = mapName,
                NodeSize = nodeSize,
                MapSize = mapSize,
                ClusterSize = clusterSize,
                ObstacleIndexes = obstacleIndexes
            };

            mapdataJsonConverter.SaveMapDataToJson(mapData);
        }
    }
}
