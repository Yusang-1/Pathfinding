using UnityEngine;
using System;

public class PathManager : MonoBehaviour
{
    public event Action OnPathFound;
    public event Action<PathResultRecorder.PathResult> OnAFound;
    public event Action<PathResultRecorder.PathResult> OnHPASmoothAStarFound;
    public event Action<PathResultRecorder.PathResult> OnHPAThetaFound;
    public event Action<PathResultRecorder.PathResult> OnHPASmoothThetaFound;

    private NodeList nodeList;
    private MapGenerator mapGenerator;
    private MapdataJsonConverter mapdataJsonConverter;
    private readonly Pathfinder pathfinder = new();

    [SerializeField] private UIRoot uiRoot;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private NodeData nodeData;
    [SerializeField] private ClusterShower clusterShower;
    [SerializeField] private LineDrawer lineDrawer;
    [SerializeField] private Assets.Scripts.ControllUnit.SO.UnitsSO unitsSO;
    [SerializeField] private UnitUncontrollable unit;

    [Header("Values")]
    private int nodeSize;
    private int clusterSize;
    private int mapSize;

    private void Start()
    {
        mapdataJsonConverter = new MapdataJsonConverter();
        nodeList = new NodeList(nodeData);        
        mapGenerator = new MapGenerator(nodePrefab, nodeList);

        nodeData.Initialize();
        pathfinder.Initialize(nodeList, clusterShower, lineDrawer, unit);

        UIRootInitialize();

        OnPathFound += () => uiRoot.ActiveResultController(true);
        OnAFound += uiRoot.SetAResult;
        OnHPASmoothAStarFound += uiRoot.SetHPASmoothAStarResult;
        OnHPAThetaFound += uiRoot.SetHPAThetaResult;
        OnHPASmoothThetaFound += uiRoot.SetHPASmoothThetaResult;

        inputManager.ControllMenu += uiRoot.ControllMenu;

        nodeList.NodeTypeController.NodeTypeDrawer.OnPathfindAvailable += (value) => uiRoot.ActiveFindButton(value);
        nodeList.OnSelected += (node) => uiRoot.ActiveNodeTypeSelector(node, true);
        nodeList.OnDeselected += (node) => uiRoot.ActiveNodeTypeSelector(node, false);
        
        pathfinder.OnPathFound += () => OnPathFound?.Invoke();
        pathfinder.OnAFound += (pathResult) => OnAFound(pathResult);
        pathfinder.OnHPASmoothAStarFound += (pathResult) => OnHPASmoothAStarFound(pathResult);
        pathfinder.OnHPAThetaFound += (pathResult) => OnHPAThetaFound(pathResult);
        pathfinder.OnHPASmoothThetaFound += (pathResult) => OnHPASmoothThetaFound(pathResult);
    }

    private void UIRootInitialize()
    {
        uiRoot.Initialize();
        uiRoot.OnFindAllPathRequested += FindAllPath;
        uiRoot.OnSetNodeTypeRequested += nodeList.NodeTypeController.NodeTypeDrawer.SetNodeType;
        uiRoot.OnGridToWorldRequested += nodeList.GridToWorld;
        uiRoot.OnLoadMapRequested += SetMapData;
        uiRoot.OnLoadMapRequested += mapGenerator.GenerateMap;
        uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;
        uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;

        uiRoot.OnShowAStarPathRequested += pathfinder.ShowAStarResult;
        uiRoot.OnShowHAPStarPathRequested += pathfinder.ShowHPASmoothingAStarResult;
        uiRoot.OnShowHPAThetaPathRequested += pathfinder.ShowHPAThetaResult;
        uiRoot.OnShowHAPStarSmoothingPathRequested += pathfinder.ShowHPASmoothingThetaResult;
        uiRoot.OnResetAllRequested += pathfinder.ResetAll;
        uiRoot.OnShowMoveUnitRequested += pathfinder.MoveUnitLazyRefine;
    }

    private void SetMapData(MapData mapData)
    {
        nodeSize = mapData.NodeSize;
        mapSize = mapData.MapSize;
        clusterSize = mapData.ClusterSize;

        nodeList.Initialize(nodeSize, mapSize);
        clusterShower.Initialize(mapSize / clusterSize, clusterSize, nodeSize);

        pathfinder.SetNodeAndCluster(mapData, unitsSO.UnitRadius);
    }

    private void FindAllPath()
    {
        pathfinder.ComparePathfinding();
    }
}
