using UnityEngine;
using System;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public event Action OnPathFound;
    public event Action<PathResultRecorder.PathResult> OnAFound;
    public event Action<PathResultRecorder.PathResult> OnHPASmoothAStarFound;
    public event Action<PathResultRecorder.PathResult> OnHPAThetaFound;
    public event Action<PathResultRecorder.PathResult> OnHPASmoothThetaFound;

    private NodeList nodeList;
    private HPAClusterList clusterList;
    private AStarPathfinder aStarPathfinder;
    private HPAPathfinder hPAPathfinder;
    private ThetaStar thetaStarPathfinder;
    private SearchWithTheClusterResult searchWithTheClusterResult;
    private MapGenerator mapGenerator;
    private MapdataJsonConverter mapdataJsonConverter;
    private ClusterPathSmoother clusterPathSmoother;
    private readonly PathfindingResultShower resultShower = new();
    private readonly PathfindingChain pathfindingChain = new();

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

    private List<ClusterResult> currentAbstractResults;

    private Dictionary<NodeType, List<Vector2Int>> aStarResult = new();
    private Dictionary<NodeType, List<Vector2Int>> hpaStarResult = new();
    private Dictionary<NodeType, List<Vector2Int>> hpaThetaResult = new();
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
        searchWithTheClusterResult = new SearchWithTheClusterResult(aStarPathfinder, thetaStarPathfinder, clusterList, nodeList);
        clusterPathSmoother = new ClusterPathSmoother(clusterList, nodeList, this);

        nodeData.Initialize();
        unit.Initialize(lineDrawer);
        pathfindingChain.Initialize(hPAPathfinder, clusterPathSmoother, searchWithTheClusterResult);

        UIRootInitialize();

        OnPathFound += () => uiRoot.ActiveResultController(true);
        OnAFound += uiRoot.SetAResult;
        OnHPASmoothAStarFound += uiRoot.SetHPASmoothAStarResult;
        OnHPAThetaFound += uiRoot.SetHPAThetaResult;
        OnHPASmoothThetaFound += uiRoot.SetHPASmoothThetaResult;

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
        uiRoot.OnShowHAPStarPathRequested += ShowHPASmoothingAStarResult;
        uiRoot.OnShowHPAThetaPathRequested += ShowHPAThetaResult;
        uiRoot.OnShowHAPStarSmoothingPathRequested += ShowHPASmoothingThetaResult;
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

    private Vector3 from, to;
    public Vector3 From => from;
    public Vector3 To => to;
    private void FindAllPath()
    {
        CreateCluster();

        from = nodeList.GridToWorld(nodeList.NodeTypeDrawer.StartNodeIndex);
        to = nodeList.GridToWorld(nodeList.NodeTypeDrawer.GoalNodeIndex);

        aStarResult = FindAStarPath();

        hpaStarResult = FindHPA_Smoothing_AStarPath();

        hpaThetaResult = FindHPA_ThetaPath();

        hpaStarSmoothResult = FindHPA_Smoothing_ThetaPath();

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

        var path = aStarPathfinder.FindPath(from, to);
        Vector3ListPool.ReleaseValue(path);

        OnAFound?.Invoke(PathResultRecorder.GetPathResult());

        var result = nodeList.NodeTypeDrawer.GetNodeInfo();
        nodeList.NodeTypeDrawer.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPA_Smoothing_AStarPath()
    {
        clusterList.ResetClusterList();
        PathResultRecorder.ResetPathResult();
        ClusterResultPool.Initialize();

        currentAbstractResults = pathfindingChain.ClusterPath_StringPulling?.Invoke((from, to));

        pathfindingChain.HPAStar_StringPulling?.Invoke((from, to));

        ClusterResultPool.ReleaseAllValue();

        OnHPASmoothAStarFound?.Invoke(PathResultRecorder.GetPathResult());

        var result = nodeList.NodeTypeDrawer.GetNodeInfo();
        nodeList.NodeTypeDrawer.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPA_ThetaPath()
    {
        clusterList.ResetClusterList();
        PathResultRecorder.ResetPathResult();
                
        pathfindingChain.HPAStar_Theta?.Invoke((from, to));

        ClusterResultPool.ReleaseAllValue();

        OnHPAThetaFound?.Invoke(PathResultRecorder.GetPathResult());

        var result = nodeList.NodeTypeDrawer.GetNodeInfo();        
        result[NodeType.trace].Add(nodeList.GetNodeIndex(to));
        
        nodeList.NodeTypeDrawer.ClearDict();
        return result;
    }

    private Dictionary<NodeType, List<Vector2Int>> FindHPA_Smoothing_ThetaPath()
    {
        clusterList.ResetClusterList();
        PathResultRecorder.ResetPathResult();

        smoothPath = pathfindingChain.HPAStar_StringPulling_Theta?.Invoke((from, to));

        ClusterResultPool.ReleaseAllValue();

        OnHPASmoothThetaFound?.Invoke(PathResultRecorder.GetPathResult());

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
    private void ShowHPASmoothingAStarResult()
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarResult);
        lineDrawer.DrawLine(hpaStarResult[NodeType.trace]);
        clusterShower.ShowActivatedClusters(currentAbstractResults);
    }
    private void ShowHPAThetaResult()
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarResult);
        lineDrawer.DrawLine(hpaThetaResult[NodeType.trace]);
        clusterShower.ShowActivatedClusters(currentAbstractResults);
    }
    private void ShowHPASmoothingThetaResult()
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
