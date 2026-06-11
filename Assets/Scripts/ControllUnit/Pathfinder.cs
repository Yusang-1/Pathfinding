using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    private NodeList nodeList;
    private HPAClusterList clusterList;

    AStarPathfinder aStarPathfinder;
    private ThetaStar thetaStarPathfinder;
    private HPAPathfinder highLevelPathfinder;
    private SearchWithTheClusterResult searchWithTheClusterResult;
    private LazyRefine lazyRefine;

    public void SetNodeAndCluster(NodeList nodes, int mapSize, int clusterSize)
    {
        nodeList = nodes;
        clusterList = new HPAClusterList(nodeList);
        aStarPathfinder = new AStarPathfinder(nodeList, clusterList);

        clusterList.Initialize(aStarPathfinder, mapSize, clusterSize);
        nodeList.SetNodeArea();

        thetaStarPathfinder = new ThetaStar(nodeList, clusterList);
        highLevelPathfinder = new HPAPathfinder(clusterList, nodeList, aStarPathfinder);
        searchWithTheClusterResult = new SearchWithTheClusterResult(aStarPathfinder, thetaStarPathfinder);
        lazyRefine = new LazyRefine(clusterList, nodeList, searchWithTheClusterResult);
    }

    public List<HPAPathfinder.ResultNode> GetAbstractPath(Vector3 from, Vector3 to)
    {
        return highLevelPathfinder.FindClusterPath(from, to, out PathResult result);
    }

    public void SearchLowLevelPath(HPAPathfinder.ResultNode resultNode)
    {
        lazyRefine.DoLazyRefinement(resultNode);
    }

    public bool TryGetShortDestination(out Vector3 path)
    {
        if (lazyRefine.TryGetPathFromQueue(out path))
        {
            return true;
        }
        else return false;
    }
}
