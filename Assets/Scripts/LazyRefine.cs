using UnityEngine;
using System.Collections.Generic;
using System;

public class LazyRefine
{
    private readonly Queue<Vector3> pathQueue = new();

    private readonly HPAPathfinder hPAPathfinder;
    private readonly ThetaStar thetaStarPathfinder;
    private readonly HPAClusterList clusterList;
    private readonly SearchWithTheClusterResult searchWithTheClusterResult;
    private readonly NodeList nodeList;
    public LazyRefine(HPAPathfinder hPAPathfinder, ThetaStar thetaStarPathfinder, HPAClusterList clusterList, NodeList nodeList, SearchWithTheClusterResult searchWithTheClusterResult)
    {
        this.hPAPathfinder = hPAPathfinder;
        this.thetaStarPathfinder = thetaStarPathfinder;
        this.clusterList = clusterList;
        this.nodeList = nodeList;
        this.searchWithTheClusterResult = searchWithTheClusterResult;
    }

    public bool TryGetPathFromQueue(out Vector3 path)
    {
        if (pathQueue.Count <= 0)
        {
            Debug.LogWarning("pathQueue가 비어있음");
            path = Vector3.zero;
            return false;
        }
        else
        {
            path = pathQueue.Dequeue();
            return true;
        }
    }

    public void DoLazyRefinement(HPAPathfinder.ResultNode result, LineDrawer lineDrawer)
    {        
        List<Vector3> resultPath = searchWithTheClusterResult.FindPathTheta(result, thetaStarPathfinder, nodeList, clusterList);
        
        lineDrawer.DrawLine(resultPath);
        
        for(int i = 0; i < resultPath.Count; i++)
        {
            pathQueue.Enqueue(resultPath[i]);
        }
    }
}
