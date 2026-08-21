using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Pathfinding;

public class PathfinderComparePathfinding
{
    public event Action<bool> OnPathFound;
    public event Action<PathResultRecorder.PathResult> OnAFound;
    public event Action<PathResultRecorder.PathResult> OnHPASmoothAStarFound;
    public event Action<PathResultRecorder.PathResult> OnHPAThetaFound;
    public event Action<PathResultRecorder.PathResult> OnHPASmoothThetaFound;

    private Action<bool> pathFoundHandler;
    private Action<PathResultRecorder.PathResult> aFoundHandler;
    private Action<PathResultRecorder.PathResult> hPASmoothAStarFoundHandler;
    private Action<PathResultRecorder.PathResult> hPAThetaFoundHandler;
    private Action<PathResultRecorder.PathResult> hPASmoothThetaFoundHandler;

    private NodeList nodeList;
    private HPAClusterList clusterList;

    private HPAPathfinder highLevelPathfinder;
    private ClusterPathSmoother clusterPathSmoother;
    private AStarPathfinder aStarPathfinder;
    private ThetaStar thetaStarPathfinder;
    private SearchWithTheClusterResult searchWithTheClusterResult;
    private ComparePathfinding comparePathfinding;
    private ComparePathfindingShower comparePathfindingShower;
    private UnitUncontrollable unit;
    private readonly ClusterResultWrapper clusterResultWrapper = new();
    private readonly PathfindingChain pathfindingChain = new();

    private bool isEventBound = false;

    public void Initialize(NodeList nodes, ClusterShower clusterShower, LineDrawer lineDrawer, UnitUncontrollable unit)
    {
        nodeList = nodes;

        aStarPathfinder = new AStarPathfinder(nodeList);
        thetaStarPathfinder = new ThetaStar(nodeList);
        clusterList = new HPAClusterList(nodeList);
        highLevelPathfinder = new HPAPathfinder(nodeList, clusterList);
        clusterPathSmoother = new ClusterPathSmoother(nodeList, clusterList);
        searchWithTheClusterResult = new SearchWithTheClusterResult(aStarPathfinder, thetaStarPathfinder, clusterList, nodeList);
        comparePathfinding = new ComparePathfinding(nodeList, clusterList, aStarPathfinder, clusterResultWrapper, pathfindingChain);
        comparePathfindingShower = new ComparePathfindingShower(nodeList, clusterList, clusterShower, lineDrawer);

        this.unit = unit;
        unit.Initialize(lineDrawer);
        clusterPathSmoother = new(nodeList, clusterList);
        pathfindingChain.Initialize(highLevelPathfinder, clusterPathSmoother, searchWithTheClusterResult);

        CreateEventHandlers();
        BindComparePathfindingEvents();
    }
    
    private void CreateEventHandlers()
    {
        pathFoundHandler = (value) => OnPathFound?.Invoke(value);
        aFoundHandler = (pathResult) => OnAFound?.Invoke(pathResult);
        hPASmoothAStarFoundHandler = (pathResult) => OnHPASmoothAStarFound?.Invoke(pathResult);
        hPAThetaFoundHandler = (pathResult) => OnHPAThetaFound?.Invoke(pathResult);
        hPASmoothThetaFoundHandler = (pathResult) => OnHPASmoothThetaFound?.Invoke(pathResult);
    }

    private void BindComparePathfindingEvents()
    {
        if (isEventBound) return;

        comparePathfinding.OnPathFound += pathFoundHandler;
        comparePathfinding.OnAFound += aFoundHandler;
        comparePathfinding.OnHPASmoothAStarFound += hPASmoothAStarFoundHandler;
        comparePathfinding.OnHPAThetaFound += hPAThetaFoundHandler;
        comparePathfinding.OnHPASmoothThetaFound += hPASmoothThetaFoundHandler;

        isEventBound = true;
    }

    public void SetNodeAndCluster(in MapData mapData, Dictionary<UnitSize, float> unitRadiusList)
    {
        clusterList.Initialize(aStarPathfinder, mapData.MapSize, mapData.ClusterSize, unitRadiusList);

        nodeList.SetNodeArea();
    }

    private void DoComparePathfinding()
    {
        Vector3 from = nodeList.GridToWorld(nodeList.NodeTypeController.NodeTypeDrawer.StartNodeIndex);
        Vector3 to = nodeList.GridToWorld(nodeList.NodeTypeController.NodeTypeDrawer.GoalNodeIndex);

        comparePathfinding.DoComparePathfinding(from, to);
    }

    public void ShowAStarResult()
    {
        comparePathfindingShower.ShowAStarResult(comparePathfinding.AStarResult);
    }

    public void ShowHPASmoothingAStarResult()
    {
        comparePathfindingShower.ShowHPASmoothingAStarResult(comparePathfinding.HpaStarResult, comparePathfinding.CurrentAbstractResults);
    }

    public void ShowHPAThetaResult()
    {
        comparePathfindingShower.ShowHPAThetaResult(comparePathfinding.HpaStarResult, comparePathfinding.HpaThetaResult, comparePathfinding.CurrentAbstractResults);
    }

    public void ShowHPASmoothingThetaResult()
    {
        comparePathfindingShower.ShowHPASmoothingThetaResult(comparePathfinding.HpaStarSmoothResult, comparePathfinding.SmoothPath, comparePathfinding.CurrentAbstractResults);
    }

    public void MoveUnitLazyRefine()
    {
        comparePathfindingShower.MoveUnitLazyRefine(unit, comparePathfinding.HpaStarSmoothResult, comparePathfinding.CurrentAbstractResults);
    }

    public void ResetAll()
    {
        nodeList.ResetAll();
        clusterList.ResetClusterList();

        comparePathfindingShower.ResetAll();
        unit.gameObject.SetActive(false);

        nodeList.NodeTypeController.NodeTypeDrawer.IsDuringNodeSetting = true;
    }

    public void FindAllPath()
    {
        DoComparePathfinding();
    }
}
