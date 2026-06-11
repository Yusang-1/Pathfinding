using UnityEngine;

public class MapController : MonoBehaviour
{
    private readonly MapGenerator mapGenerator = new();
    private NodeList nodeList;

    [SerializeField] private Pathfinder pathfinder;
    [SerializeField] private ControllUnitUIRoot uiRoot;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private NodeData nodeData;

    [Header("Values")]
    [SerializeField] private int nodeSize;
    private int mapSize;
    private int clusterSize;
    private const int defaultMapSize = 20;
    private const int maxClusterSize = 10;

    private void Start()
    {
        nodeData.Initialize();
        nodeList = new NodeList(nodeSize, nodeData);


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

        nodeList.CreateNodeArray(mapSize);
        mapGenerator.GenerateMap(mapSize, nodePrefab, nodeList);

        pathfinder.SetNodeAndCluster(nodeList, mapSize, clusterSize);
    }
}
