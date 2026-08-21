using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Pathfinding;

public class LazyRefine
{
    private readonly Queue<Vector3> pathQueue = new();

    private readonly HPAClusterList clusterList;
    private readonly SearchWithTheClusterResult searchWithTheClusterResult;
    private readonly NodeList nodeList;
    public LazyRefine(HPAClusterList clusterList, NodeList nodeList, SearchWithTheClusterResult searchWithTheClusterResult)
    {
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

    public void DoLazyRefinement(ClusterSmootherResult smoothPath, LineDrawer lineDrawer)
    {
        float tempUnitRadius = 0;
        List<Vector3> resultPath = searchWithTheClusterResult.FindPathThetaWithClusterList(smoothPath, nodeList, clusterList, tempUnitRadius);

        lineDrawer.DrawLine(resultPath);

        for (int i = 0; i < resultPath.Count; i++)
        {
            pathQueue.Enqueue(resultPath[i]);
        }
        
        Vector3ListPool.ReleaseValue(resultPath);
    }
}
