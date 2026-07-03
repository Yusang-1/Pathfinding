using UnityEngine;
using System;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public event Action OnPathFound;
    public event Action<PathResult> OnAFound;
    public event Action<PathResult> OnHPAFound;
    public event Action<PathResult> OnHPASmoothFound;

    private NodeList nodeList;
    private HPAClusterList clusterList;
    private AStarPathfinder aStarPathfinder;
    private HPAPathfinder hPAPathfinder;
    private ThetaStar thetaStarPathfinder;
    private SearchWithTheClusterResult searchWithTheClusterResult;
    private MapGenerator mapGenerator;
    private MapdataJsonConverter mapdataJsonConverter;
    private readonly PathfindingResultShower resultShower = new();
    private readonly ClusterPathSmoother clusterPathSmoother = new();

    [SerializeField] private UIRoot uiRoot;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private NodeData nodeData;
    [SerializeField] private ClusterShower clusterShower;
    [SerializeField] private LineDrawer lineDrawer;

    [SerializeField] private UnitUncontrollable unit;

    [Header("Values")]
    private int nodeSize;
    private int clusterSize;
    private int mapSize;
    private bool isMapGenerated = false;

    private List<ClusterSmootherResult> currentAbstractResults;

    private Dictionary<NodeType, List<Vector2Int>> aStarResult = new();
    private Dictionary<NodeType, List<Vector2Int>> hpaStarResult = new();
    private Dictionary<NodeType, List<Vector2Int>> hpaStarSmoothResult = new();
    private List<Vector3> smoothPath;

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
        unit.Initialize(lineDrawer);

        UIRootInitialize();

        OnPathFound += () => uiRoot.ActiveResultController(true);
        OnAFound += uiRoot.SetAResult;
        OnHPAFound += uiRoot.SetHPAResult;
        OnHPASmoothFound += uiRoot.SetHPASmoothResult;

        inputManager.ControllMenu += uiRoot.ControllMenu;

        nodeList.NodeInfo.OnPathfindAvailable += (value) => uiRoot.ActiveFindButton(value);
        nodeList.OnSelected += (node) => uiRoot.ActiveNodeTypeSelector(node, true);
        nodeList.OnDeselected += (node) => uiRoot.ActiveNodeTypeSelector(node, false);
    }

    private void UIRootInitialize()
    {
        uiRoot.Initialize();
        uiRoot.OnFindAllPathRequested += FindAllPath;
        uiRoot.OnSetNodeTypeRequested += nodeList.NodeInfo.SetNodeType;
        uiRoot.OnGridToWorldRequested += nodeList.GridToWorld;
        uiRoot.OnLoadMapRequested += SetMapData;
        uiRoot.OnLoadMapRequested += mapGenerator.GenerateMap;
        uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;
        uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;

        uiRoot.OnShowAStarPathRequested += ShowAStarResult;
        uiRoot.OnShowHAPStarPathRequested += ShowHPAStarResult;
        uiRoot.OnShowHAPStarSmoothingPathRequested += ShowHPAStarSmoothingResult;
        uiRoot.OnResetAllRequested += ResetAll;
        uiRoot.OnShowMoveUnitRequested += MoveUnitLazyRefine;
    }

    private void SetMapData(MapData mapData)
    {
        nodeSize = mapData.NodeSize;
        mapSize = mapData.MapSize;
        clusterSize = mapData.ClusterSize;

        nodeList.Initialize(nodeSize, mapSize);
        clusterShower.Initialize(mapSize / clusterSize, clusterSize, nodeSize);

        isMapGenerated = true;
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

        clusterList.Initialize(aStarPathfinder, mapSize, clusterSize);
        nodeList.SetNodeArea();
    }

    private Dictionary<NodeType, List<Vector2Int>> FindAStarPath()
    {
        clusterList.SetAllCLusterActive();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeInfo.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeInfo.GoalNodeIndex);

        aStarPathfinder.FindPath(from, to, out PathResult pathResult);

        OnAFound?.Invoke(pathResult);

        var result = nodeList.NodeInfo.GetNodeInfo();
        nodeList.NodeInfo.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPAStarPath()
    {
        clusterList.ResetClusterList();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeInfo.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeInfo.GoalNodeIndex);
        var clusterResult = hPAPathfinder.FindClusterPath(from, to, out PathResult clusterPathResult);
        if (clusterResult == null) return null;

        var clusterSmoothResult = clusterPathSmoother.SmoothClusterPath(from, to, clusterResult, clusterList, nodeList);
        if (clusterSmoothResult == null) return null;

        currentAbstractResults = clusterSmoothResult;

        searchWithTheClusterResult.FindPath(clusterSmoothResult, nodeList, clusterList, out PathResult nodePathResult);

        clusterPathResult.AddResult(nodePathResult);
        OnHPAFound?.Invoke(clusterPathResult);

        var result = nodeList.NodeInfo.GetNodeInfo();
        nodeList.NodeInfo.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPAStarPathSmoothing()
    {
        clusterList.ResetClusterList();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeInfo.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeInfo.GoalNodeIndex);
        var clusterResult = hPAPathfinder.FindClusterPath(from, to, out PathResult clusterPathResult);
        if (clusterResult == null) return null;

        var clusterSmoothResult = clusterPathSmoother.SmoothClusterPath(from, to, clusterResult, clusterList, nodeList);
        if (clusterSmoothResult == null) return null;

        currentAbstractResults = clusterSmoothResult;

        smoothPath = searchWithTheClusterResult.FindPathTheta(clusterSmoothResult, nodeList, clusterList, out PathResult nodePathResult);

        clusterPathResult.AddResult(nodePathResult);
        OnHPASmoothFound?.Invoke(clusterPathResult);

        var result = nodeList.NodeInfo.GetNodeInfo();
        nodeList.NodeInfo.ClearDict();
        return result;
    }

    private void ResetPath()
    {
        nodeList.ResetTrace();
        clusterList.ResetClusterList();
        clusterShower.ResetClusters();
        lineDrawer.ResetLineDrawer();
    }
    private void ResetAll()
    {
        nodeList.ResetAll();
        clusterList.ResetClusterList();
        clusterShower.ResetAllClusters();
        lineDrawer.ResetLineDrawer();
        unit.gameObject.SetActive(false);

        nodeList.NodeInfo.IsDuringNodeSetting = true;
    }

    private void ShowAStarResult()
    {
        ResetPath();
        resultShower.DrawAStar(nodeList, aStarResult);
        lineDrawer.DrawLine(aStarResult[NodeType.trace]);
    }
    private void ShowHPAStarResult()
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarResult);
        lineDrawer.DrawLine(hpaStarResult[NodeType.trace]);
        clusterShower.ShowActivatedClusters(currentAbstractResults);
    }
    private void ShowHPAStarSmoothingResult()
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarSmoothResult);
        clusterShower.ShowActivatedClusters(currentAbstractResults);
        lineDrawer.DrawLine(smoothPath);
    }

    private void MoveUnitLazyRefine()
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarSmoothResult);

        unit.MoveWithResult(currentAbstractResults, clusterList, nodeList, searchWithTheClusterResult);
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
