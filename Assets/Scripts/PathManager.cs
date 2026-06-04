using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    private NodeList nodeList;
    private HPAClusterList hPAClusterList;
    private AStarPathfinder pathfinder;
    private HPAPathfinder hPAPathfinder;
    private ThetaStar thetaStar;
    private MapGenerator mapGenerator;
    private SearchWithTheClusterResult searchWithTheClusterResult;
    private readonly PathfindingResultShower resultShower = new();

    [SerializeField] private UIManager uiManager;

    [SerializeField] private Node nodePrefab;
    [SerializeField] private NodeData nodeData;
    [SerializeField] private ClusterShower clusterShower;
    [SerializeField] private LineDrawer lineDrawer;

    [SerializeField] private Unit unit;

    [Header("Values")]
    [SerializeField] private int nodeSize;
    [SerializeField] private int clusterSize;
    [SerializeField] private int mapSize;

    private bool isMapGenerated = false;
    private List<HPAPathfinder.ResultNode> currentAbstractResults;

    private Dictionary<NodeType, List<Vector2Int>> aStarResult = new();
    private Dictionary<NodeType, List<Vector2Int>> hpaStarResult = new();
    private List<HPAPathfinder.ResultNode> clusterResult = new();
    private Dictionary<NodeType, List<Vector2Int>> hpaStarSmoothResult = new();
    private List<Vector3> smoothPath;

    private void Start()
    {
        nodeData.Initialize();
        nodeList = new NodeList(nodeSize, nodeData);
        hPAClusterList = new(nodeList);

        lineDrawer.Initialize();
        uiManager.Initialize(nodeList, GenerateMap, FindAllPath, ResetAll);
        unit.Initialize(lineDrawer);

        pathfinder = new AStarPathfinder(nodeList, hPAClusterList);
        thetaStar = new ThetaStar(nodeList, hPAClusterList);

        searchWithTheClusterResult = new SearchWithTheClusterResult();

        mapGenerator = new MapGenerator(nodePrefab);

        nodeList.NodeInfo.OnPathfindAvailable += uiManager.ActiveFindUI;
    }

    private const int defaultMapSize = 20;
    private const int maxClusterSize = 10;
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
            clusterSize = sizeOfMap / 4;
            clusterSize = Mathf.Clamp(clusterSize, 0, maxClusterSize);
        }
        else
        {
            clusterSize = sizeOfCluster;
        }

        clusterShower.Initialize(clusterSize, nodeSize);
        hPAPathfinder = new HPAPathfinder(clusterSize, hPAClusterList, nodeList, pathfinder);
        nodeList.CreateNodeArray(mapSize);
        mapGenerator.GenerateMap(mapSize, nodeList);
        isMapGenerated = true;
    }

    private void FindAllPath()
    {
        CreateCluster();

        aStarResult = FindAStarPath();

        hpaStarResult = FindHPAStarPath();

        hpaStarSmoothResult = FindHPAStarPathSmoothing();

        nodeList.NodeInfo.IsDuringNodeSetting = false;
    }

    private void CreateCluster()
    {
        if (!isMapGenerated) return;

        hPAClusterList.Initialize(pathfinder, mapSize, clusterSize);
        nodeList.SetNodeArea();
    }

    private Dictionary<NodeType, List<Vector2Int>> FindAStarPath()
    {
        hPAClusterList.SetAllCLusterActive();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeInfo.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeInfo.GoalNodeIndex);

        pathfinder.FindPath(from, to);

        // nodeList.NodeInfo.ShowAStarPath();
        var result = nodeList.NodeInfo.DeepCopy();
        nodeList.NodeInfo.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPAStarPath()
    {
        hPAClusterList.ResetClusterList();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeInfo.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeInfo.GoalNodeIndex);
        clusterResult = hPAPathfinder.FindClusterPath(from, to);
        if (clusterResult == null) return null;
        currentAbstractResults = clusterResult;
        // clusterShower.ShowActivatedClusters(clusterResult);

        searchWithTheClusterResult.FindPath(clusterResult, pathfinder, nodeList, hPAClusterList);

        // nodeList.NodeInfo.ShowHPAStarPath();
        var result = nodeList.NodeInfo.DeepCopy();
        nodeList.NodeInfo.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPAStarPathSmoothing()
    {
        hPAClusterList.ResetClusterList();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeInfo.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeInfo.GoalNodeIndex);
        clusterResult = hPAPathfinder.FindClusterPath(from, to);
        if (clusterResult == null) return null;
        currentAbstractResults = clusterResult;
        // clusterShower.ShowActivatedClusters(clusterResult);

        smoothPath = searchWithTheClusterResult.FindPathTheta(clusterResult, thetaStar, nodeList, hPAClusterList);
        // lineDrawer.DrawLine(smoothPath);

        // nodeList.NodeInfo.ShowHPAStarPath();
        var result = nodeList.NodeInfo.DeepCopy();
        nodeList.NodeInfo.ClearDict();
        return result;
    }

    private void ResetPath()
    {
        nodeList.ResetTrace();
        hPAClusterList.ResetClusterList();
        clusterShower.ResetClusters();
        lineDrawer.ResetLineDrawer();
    }
    private void ResetAll()
    {
        nodeList.ResetAll();
        hPAClusterList.ResetClusterList();
        clusterShower.ResetClusters();
        lineDrawer.ResetLineDrawer();

        nodeList.NodeInfo.IsDuringNodeSetting = true;
    }

    public void ShowAStarResult()
    {
        ResetPath();
        resultShower.DrawAStar(nodeList, aStarResult);
    }
    public void ShowHPAStarResult()
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarResult);
        clusterShower.ShowActivatedClusters(clusterResult);
    }
    public void ShowHPAStarSmoothingResult()
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarSmoothResult);
        clusterShower.ShowActivatedClusters(clusterResult);
        lineDrawer.DrawLine(smoothPath);
    }

    public void MoveUnitLazyRefine()
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarSmoothResult);

        unit.MoveWithResult(currentAbstractResults, hPAPathfinder, thetaStar, hPAClusterList, nodeList, searchWithTheClusterResult);
    }
}
