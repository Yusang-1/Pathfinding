using UnityEngine;
using System;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public event Action OnPathFound;
    public event Action<PathResultRecorder.PathResult> OnAFound;
    public event Action<PathResultRecorder.PathResult> OnHPAFound;
    public event Action<PathResultRecorder.PathResult> OnHPASmoothFound;

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

        nodeList.NodeTypeDrawer.OnPathfindAvailable += (value) => uiRoot.ActiveFindButton(value);
        nodeList.OnSelected += (node) => uiRoot.ActiveNodeTypeSelector(node, true);
        nodeList.OnDeselected += (node) => uiRoot.ActiveNodeTypeSelector(node, false);
    }

    private void UIRootInitialize()
    {
        uiRoot.Initialize();
        uiRoot.OnFindAllPathRequested += FindAllPath;
        uiRoot.OnSetNodeTypeRequested += nodeList.NodeTypeDrawer.SetNodeType;
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

        nodeList.NodeTypeDrawer.IsDuringNodeSetting = false;
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
        PathResultRecorder.ResetPathResult();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeTypeDrawer.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeTypeDrawer.GoalNodeIndex);

        aStarPathfinder.FindPath(from, to);

        OnAFound?.Invoke(PathResultRecorder.GetPathResult());

        var result = nodeList.NodeTypeDrawer.GetNodeInfo();
        nodeList.NodeTypeDrawer.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPAStarPath()
    {
        clusterList.ResetClusterList();
        PathResultRecorder.ResetPathResult();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeTypeDrawer.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeTypeDrawer.GoalNodeIndex);
        var clusterResult = hPAPathfinder.FindClusterPath(from, to);
        if (clusterResult == null) return null;

        var clusterSmoothResult = clusterPathSmoother.SmoothClusterPath(from, to, clusterResult, clusterList, nodeList);
        if (clusterSmoothResult == null) return null;

        currentAbstractResults = clusterSmoothResult;
        
        PathResultRecorder.ResetPathLength();
        searchWithTheClusterResult.FindPath(clusterSmoothResult, nodeList, clusterList);

        OnHPAFound?.Invoke(PathResultRecorder.GetPathResult());

        var result = nodeList.NodeTypeDrawer.GetNodeInfo();
        nodeList.NodeTypeDrawer.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPAStarPathSmoothing()
    {
        clusterList.ResetClusterList();
        PathResultRecorder.ResetPathResult();

        Vector3 from = nodeList.GridToWorld(nodeList.NodeTypeDrawer.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeTypeDrawer.GoalNodeIndex);
        var clusterResult = hPAPathfinder.FindClusterPath(from, to);
        if (clusterResult == null) return null;

        var clusterSmoothResult = clusterPathSmoother.SmoothClusterPath(from, to, clusterResult, clusterList, nodeList);
        if (clusterSmoothResult == null) return null;

        currentAbstractResults = clusterSmoothResult;
        
        PathResultRecorder.ResetPathLength();
        smoothPath = searchWithTheClusterResult.FindPathTheta(clusterSmoothResult, nodeList, clusterList);

        OnHPASmoothFound?.Invoke(PathResultRecorder.GetPathResult());

        var result = nodeList.NodeTypeDrawer.GetNodeInfo();
        nodeList.NodeTypeDrawer.ClearDict();
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

        nodeList.NodeTypeDrawer.IsDuringNodeSetting = true;
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
