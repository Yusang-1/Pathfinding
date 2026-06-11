using System;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public event Action OnMapGenerated;
    
    private readonly MapGenerator mapGenerator = new();
    private NodeList nodeList;
    private HPAClusterList clusterList;
    private AStarPathfinder aStarPathfinder;
    private ThetaStar thetaStarPathfinder;
    private HPAPathfinder highLevelPathfinder;
    private SearchWithTheClusterResult searchWithTheClusterResult;
    
    [SerializeField] private ControllUnitUIRoot uiRoot;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private NodeData nodeData;
    
    [Header("Values")]
    [SerializeField] private int nodeSize;    
    private int mapSize;
    private int clusterSize;
    // private bool isMapGenerated;
    private const int defaultMapSize = 20;
    private const int maxClusterSize = 10;

    private void Start()
    {
        nodeData.Initialize();
        nodeList = new NodeList(nodeSize, nodeData);
        clusterList = new(nodeList);
        
        aStarPathfinder = new AStarPathfinder(nodeList, clusterList);
        thetaStarPathfinder = new ThetaStar(nodeList, clusterList);
        searchWithTheClusterResult = new SearchWithTheClusterResult(aStarPathfinder, thetaStarPathfinder);
        
        uiRoot.Initialize();
        uiRoot.OnGenerateMapRequested += GenerateMap;
    }
        
    private void GenerateMap(int sizeOfMap, int sizeOfCluster)
    {
        if (sizeOfMap == 0)
        {
            mapSize = defaultMapSize;
        }
        else
            mapSize = sizeOfMap;

        if (sizeOfCluster == 0)
        {
            clusterSize = mapSize / 4;
            clusterSize = Mathf.Clamp(clusterSize, 0, maxClusterSize);
        }
        else
        {
            clusterSize = sizeOfCluster;
        }
        
        highLevelPathfinder = new HPAPathfinder(clusterList, nodeList, aStarPathfinder);
        nodeList.CreateNodeArray(mapSize);
        mapGenerator.GenerateMap(mapSize, nodePrefab, nodeList);
        // isMapGenerated = true;

        OnMapGenerated?.Invoke();
    }
}
