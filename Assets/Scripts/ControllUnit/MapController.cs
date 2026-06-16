using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.ControllUnit.UI;

namespace Assets.Scripts.ControllUnit
{
    public class MapController : MonoBehaviour
    {
        private AStarPathfinder aStarPathfinder;
        private HPAPathfinder hPAPathfinder;
        private ThetaStar thetaStarPathfinder;
        private SearchWithTheClusterResult searchWithTheClusterResult;

        private MapGenerator mapGenerator;
        private NodeList nodeList;
        private HPAClusterList clusterList;
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
        private bool isMapGenerated = false;

        private void Start()
        {
            mapdataJsonConverter = new MapdataJsonConverter();
            nodeList = new NodeList(nodeData);
            clusterList = new HPAClusterList(nodeList);
            mapGenerator = new MapGenerator(nodePrefab, nodeList);
            aStarPathfinder = new AStarPathfinder(nodeList, clusterList);
            hPAPathfinder = new HPAPathfinder(clusterList, nodeList, aStarPathfinder);
            thetaStarPathfinder = new ThetaStar(nodeList, clusterList);
            searchWithTheClusterResult = new SearchWithTheClusterResult(aStarPathfinder, thetaStarPathfinder);

            nodeData.Initialize();



            uiRoot.OnLoadMapRequested += SetMapData;
            uiRoot.OnLoadMapRequested += mapGenerator.GenerateMap;
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

            isMapGenerated = true;
        }

        private void CreateCluster()
        {
            if (!isMapGenerated) return;

            clusterList.Initialize(aStarPathfinder, mapSize, clusterSize);
            nodeList.SetNodeArea();
        }

        private void FindPath()
        {
            var hpaStarSmoothResult = FindHighLevelPath();
        }

        private List<HPAPathfinder.ResultNode> FindHighLevelPath()
        {
            clusterList.ResetClusterList();

            Vector3 from = nodeList.GridToWorld(nodeList.NodeInfo.StartNodeIndex);
            Vector3 to = nodeList.GridToWorld(nodeList.NodeInfo.GoalNodeIndex);
            var clusterResult = hPAPathfinder.FindClusterPath(from, to, out PathResult clusterPathResult);
            return clusterResult;
        }
    }
}
