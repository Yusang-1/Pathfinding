using UnityEngine;
using System.Collections.Generic;

public class ComparePathfindingShower
{
    private readonly NodeList nodeList;
    private readonly HPAClusterList clusterList;

    private readonly PathfindingResultShower resultShower = new();
    private readonly LineDrawer lineDrawer;
    private readonly ClusterShower clusterShower;
    private SearchWithTheClusterResult searchWithTheClusterResult;

    public ComparePathfindingShower(NodeList nodeList, HPAClusterList clusterList, ClusterShower clusterShower, LineDrawer lineDrawer)
    {
        this.nodeList = nodeList;
        this.clusterList = clusterList;
        this.clusterShower = clusterShower;
        this.lineDrawer = lineDrawer;
    }

    public void ShowAStarResult(Dictionary<NodeType, List<Vector2Int>> aStarResult)
    {
        ResetPath();
        resultShower.DrawAStar(nodeList, aStarResult);
        lineDrawer.DrawLine(aStarResult[NodeType.trace]);
    }
    public void ShowHPASmoothingAStarResult(Dictionary<NodeType, List<Vector2Int>> hpaStarResult, ClusterResultWrapper currentAbstractResults)
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarResult);
        lineDrawer.DrawLine(hpaStarResult[NodeType.trace]);
        clusterShower.ShowActivatedClusters(currentAbstractResults);
    }
    public void ShowHPAThetaResult(Dictionary<NodeType, List<Vector2Int>> hpaStarResult, Dictionary<NodeType, List<Vector2Int>> hpaThetaResult, ClusterResultWrapper currentAbstractResults)
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarResult);
        lineDrawer.DrawLine(hpaThetaResult[NodeType.trace]);
        clusterShower.ShowActivatedClusters(currentAbstractResults);
    }
    public void ShowHPASmoothingThetaResult(Dictionary<NodeType, List<Vector2Int>> hpaStarSmoothResult, List<Vector3> smoothPath, ClusterResultWrapper currentAbstractResults)
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarSmoothResult);
        clusterShower.ShowActivatedClusters(currentAbstractResults);
        lineDrawer.DrawLine(smoothPath);
    }

    public void MoveUnitLazyRefine(UnitUncontrollable unit, Dictionary<NodeType, List<Vector2Int>> hpaStarSmoothResult, ClusterResultWrapper currentAbstractResults)
    {
        ResetPath();
        resultShower.DrawHPAStar(nodeList, hpaStarSmoothResult);

        unit.MoveWithResult(currentAbstractResults, clusterList, nodeList, searchWithTheClusterResult);
    }

    private void ResetPath()
    {
        nodeList.NodeTypeController.ResetTrace();
        clusterList.ResetClusterList();
        clusterShower.ResetClusters();
        lineDrawer.ResetLineDrawer();
    }
    
    public void ResetAll()
    {
        clusterShower.ResetAllClusters();
        lineDrawer.ResetLineDrawer();
    }
}
