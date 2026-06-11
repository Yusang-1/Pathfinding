using UnityEngine;
using System;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public event Action OnPathFound;
    public event Action OnMapGenerated;
    public event Action<PathResult> OnAFound;
    public event Action<PathResult> OnHPAFound;
    public event Action<PathResult> OnHPASmoothFound;

    private NodeList nodeList;
    private HPAClusterList hPAClusterList;
    private AStarPathfinder pathfinder;
    private HPAPathfinder hPAPathfinder;
    private ThetaStar thetaStarPathfinder;
    private SearchWithTheClusterResult searchWithTheClusterResult;
    private readonly MapGenerator mapGenerator = new();
    private readonly PathfindingResultShower resultShower = new();

    [SerializeField] private UIRoot uiRoot;

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
        UIRootInitialize();
        unit.Initialize(lineDrawer);

        pathfinder = new AStarPathfinder(nodeList, hPAClusterList);
        thetaStarPathfinder = new ThetaStar(nodeList, hPAClusterList);

        searchWithTheClusterResult = new SearchWithTheClusterResult(pathfinder, thetaStarPathfinder);

        nodeList.NodeInfo.OnPathfindAvailable += (value) => uiRoot.ActiveFindButton(value);
        nodeList.OnSelected += (index) => uiRoot.ActiveNodeTypeSelector(index, true);
        nodeList.OnDeselected += (index) => uiRoot.ActiveNodeTypeSelector(index, false);
        OnPathFound += () => uiRoot.ActiveResultController(true);
        OnMapGenerated += () => uiRoot.ActiveFindButton(true);

        OnAFound += uiRoot.SetAResult;
        OnHPAFound += uiRoot.SetHPAResult;
        OnHPASmoothFound += uiRoot.SetHPASmoothResult;
    }

    private void UIRootInitialize()
    {
        uiRoot.Initialize();
        uiRoot.OnGenerateMapRequested += GenerateMap;
        uiRoot.OnFindAllPathRequested += FindAllPath;
        uiRoot.OnResetAllRequested += ResetAll;
        uiRoot.OnSetNodeTypeRequested += nodeList.NodeInfo.SetNodeType;
        uiRoot.OnGridToWorldRequested += nodeList.GridToWorld;
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
            clusterSize = mapSize / 4;
            clusterSize = Mathf.Clamp(clusterSize, 0, maxClusterSize);
        }
        else
        {
            clusterSize = sizeOfCluster;
        }

        clusterShower.Initialize(clusterSize, nodeSize);
        hPAPathfinder = new HPAPathfinder(hPAClusterList, nodeList, pathfinder);
        nodeList.CreateNodeArray(mapSize);
        mapGenerator.GenerateMap(mapSize, nodePrefab, nodeList);
        isMapGenerated = true;

        OnMapGenerated?.Invoke();
    }

    private void FindAllPath()
    {
        CreateCluster();

        aStarResult = FindAStarPath();

        hpaStarResult = FindHPAStarPath();

        hpaStarSmoothResult = FindHPAStarPathSmoothing();

        nodeList.NodeInfo.IsDuringNodeSetting = false;
        OnPathFound?.Invoke();
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

        pathfinder.FindPath(from, to, out PathResult pathResult);

        OnAFound?.Invoke(pathResult);

        var result = nodeList.NodeInfo.GetNodeInfo();
        nodeList.NodeInfo.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPAStarPath()
    {
        hPAClusterList.ResetClusterList();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeInfo.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeInfo.GoalNodeIndex);
        clusterResult = hPAPathfinder.FindClusterPath(from, to, out PathResult clusterPathResult);
        if (clusterResult == null) return null;
        currentAbstractResults = clusterResult;

        searchWithTheClusterResult.FindPath(clusterResult, nodeList, hPAClusterList, out PathResult nodePathResult);

        clusterPathResult.AddResult(nodePathResult);
        OnHPAFound?.Invoke(clusterPathResult);

        var result = nodeList.NodeInfo.GetNodeInfo();
        nodeList.NodeInfo.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPAStarPathSmoothing()
    {
        hPAClusterList.ResetClusterList();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeInfo.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeInfo.GoalNodeIndex);
        clusterResult = hPAPathfinder.FindClusterPath(from, to, out PathResult clusterPathResult);
        if (clusterResult == null) return null;
        currentAbstractResults = clusterResult;

        smoothPath = searchWithTheClusterResult.FindPathTheta(clusterResult, nodeList, hPAClusterList, out PathResult nodePathResult);

        clusterPathResult.AddResult(nodePathResult);
        OnHPASmoothFound?.Invoke(clusterPathResult);

        var result = nodeList.NodeInfo.GetNodeInfo();
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
        unit.gameObject.SetActive(false);
        
        nodeList.NodeInfo.IsDuringNodeSetting = true;
    }

    public void ShowAStarResult()
    {
        ResetPath();
        resultShower.DrawAStar(nodeList, aStarResult);
        lineDrawer.DrawLine(aStarResult[NodeType.trace]);
    }
    public void ShowHPAStarResult()
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarResult);
        lineDrawer.DrawLine(hpaStarResult[NodeType.trace]);
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

        unit.MoveWithResult(currentAbstractResults, hPAClusterList, nodeList, searchWithTheClusterResult);
    }
}

public struct PathResult
{
    public int SearchedCount;
    public float PathLength;
    public int MemoryUsed;

    public void AddResult(PathResult addResult)
    {
        SearchedCount += addResult.SearchedCount;
        PathLength += addResult.PathLength;
        MemoryUsed += addResult.MemoryUsed;
    }
}
