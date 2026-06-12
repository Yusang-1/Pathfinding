using UnityEngine;
using Assets.Scripts.CreateMap.UI;

namespace Assets.Scripts.CreateMap
{
    public class CreateMapManager : MonoBehaviour
    {
        [SerializeField] private Node nodePrefab;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private CreateMapUIRoot uiRoot;

        private NodeList nodeList;
        private readonly MapGenerator mapGenerator = new();
        private readonly NodeTypeController nodeTypeController = new();
        private readonly MapdataJsonConverter mapdataJsonConverter = new();

        [SerializeField] private int nodeSize;
        private int mapSize;
        private int clusterSize;

        private void Start()
        {
            nodeData.Initialize();

            nodeList = new NodeList(nodeData);
            nodeList.OnSelected += nodeTypeController.SetNodeType;

            uiRoot.OnGenerateMapRequested += CreateEmptyMap;
            uiRoot.OnTileSelectorRequested += nodeTypeController.SetCurrentSelected;
            uiRoot.OnExportMapRequested += ExportMap;
            uiRoot.Initialize();

            nodeTypeController.Initialize(nodeList);
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

            nodeList.CreateNodeArray(mapSize);
            mapGenerator.GenerateMap(mapSize, nodePrefab, nodeList);
        }

        public void ExportMap(string mapName)
        {
            Vector2Int[] obstacleIndexes = nodeList.NodeInfo.GetNodeInfo();

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

        [System.Serializable]
        public struct MapData
        {
            public string MapName;
            public int NodeSize;
            public int MapSize;
            public int ClusterSize;
            public Vector2Int[] ObstacleIndexes;
        }
    }
}
