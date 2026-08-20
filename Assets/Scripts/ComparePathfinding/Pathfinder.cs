using UnityEngine;
using System;
using System.Collections.Generic;

public class Pathfinder
{
    public event Action OnPathFound;
    public event Action<PathResultRecorder.PathResult> OnAFound;
    public event Action<PathResultRecorder.PathResult> OnHPASmoothAStarFound;
    public event Action<PathResultRecorder.PathResult> OnHPAThetaFound;
    public event Action<PathResultRecorder.PathResult> OnHPASmoothThetaFound;

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

    private bool isEventSet = false;

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
        
        ConnectEvents();
    }

    private void ConnectEvents()
    {
        if (isEventSet) return;

        comparePathfinding.OnPathFound += () => OnPathFound?.Invoke();
        comparePathfinding.OnAFound += (pathResult) => OnAFound?.Invoke(pathResult);
        comparePathfinding.OnHPASmoothAStarFound += (pathResult) => OnHPASmoothAStarFound?.Invoke(pathResult);
        comparePathfinding.OnHPAThetaFound += (pathResult) => OnHPAThetaFound?.Invoke(pathResult);
        comparePathfinding.OnHPASmoothThetaFound += (pathResult) => OnHPASmoothThetaFound?.Invoke(pathResult);

        isEventSet = true;
    }

    private void DisconnectEvents()
    {
        if (!isEventSet) return;

        comparePathfinding.OnPathFound -= () => OnPathFound?.Invoke();
        comparePathfinding.OnAFound -= (pathResult) => OnAFound?.Invoke(pathResult);
        comparePathfinding.OnHPASmoothAStarFound -= (pathResult) => OnHPASmoothAStarFound?.Invoke(pathResult);
        comparePathfinding.OnHPAThetaFound -= (pathResult) => OnHPAThetaFound?.Invoke(pathResult);
        comparePathfinding.OnHPASmoothThetaFound -= (pathResult) => OnHPASmoothThetaFound?.Invoke(pathResult);

        isEventSet = false;
    }

    public void SetNodeAndCluster(in MapData mapData, Dictionary<UnitSize, float> unitRadiusList)
    {
        clusterList.Initialize(aStarPathfinder, mapData.MapSize, mapData.ClusterSize, unitRadiusList);

        nodeList.SetNodeArea();
    }

    public void ComparePathfinding()
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
}
