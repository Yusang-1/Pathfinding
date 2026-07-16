using UnityEngine;
using System.Collections.Generic;

public class SearchWithTheClusterResult
{
    private readonly AStarPathfinder aStarPathfinder;
    private readonly ThetaStar thetaStarPathfinder;
    private readonly HPAClusterList clusterList;
    private readonly NodeList nodeList;
    
    private readonly List<Vector3> resultPath = new();
    public SearchWithTheClusterResult(AStarPathfinder aStarPathfinder, ThetaStar thetaStarPathfinder, HPAClusterList clusterList, NodeList nodeList)
    {
        this.aStarPathfinder = aStarPathfinder;
        this.thetaStarPathfinder = thetaStarPathfinder;
        this.clusterList = clusterList;
        this.nodeList = nodeList;
    }

    public List<Vector3> FindPath(List<ClusterSmootherResult> pathData)
    {
        resultPath.Clear();
        foreach (var data in pathData)
        {
            for (int i = 0; i < data.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(data.ClusterIndexes[i], true);
            }

            Vector3 entrancePosition = nodeList.GridToWorld(data.EnterNodeIndex);
            Vector3 goalPosition = nodeList.GridToWorld(data.ExitNodeIndex);

            List<Vector3> pathInCluster = aStarPathfinder.FindPathInClusterList(entrancePosition, goalPosition, data.ClusterIndexes);

            PathResultRecorder.AddPathLength(1); // cluster이동 비용 1;

            resultPath.AddRange(pathInCluster);
            Vector3ListPool.ReleaseValue(pathInCluster);

            for (int i = 0; i < data.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(data.ClusterIndexes[i], false);
            }
        }

        PathResultRecorder.AddPathLength(-1); // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
        return resultPath;
    }

    public List<Vector3> FindPathTheta(List<ClusterSmootherResult> pathData)
    {
        resultPath.Clear();
        
        foreach (var data in pathData)
        {
            for (int i = 0; i < data.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(data.ClusterIndexes[i], true);
            }

            Vector3 entrancePosition = nodeList.GridToWorld(data.EnterNodeIndex);
            Vector3 goalPosition = nodeList.GridToWorld(data.ExitNodeIndex);

            List<Vector3> pathInCluster = thetaStarPathfinder.FindPathInClusterList(entrancePosition, goalPosition, data.ClusterIndexes);

            PathResultRecorder.AddPathLength(1); // cluster이동 비용 1;

            if (pathInCluster == null) continue;
            resultPath.AddRange(pathInCluster);
            Vector3ListPool.ReleaseValue(pathInCluster);

            for (int i = 0; i < data.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(data.ClusterIndexes[i], false);
            }
        }

        PathResultRecorder.AddPathLength(-1); // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
        return resultPath;
    }

    public List<Vector3> FindPathThetaWithClusterList(ClusterSmootherResult data, NodeList nodeList, HPAClusterList clusterList)
    {
        for (int i = 0; i < data.ClusterIndexes.Count; i++)
        {
            clusterList.SetClusterActive(data.ClusterIndexes[i], true);
        }

        Vector3 entrancePosition = nodeList.GridToWorld(data.EnterNodeIndex);
        Vector3 goalPosition = nodeList.GridToWorld(data.ExitNodeIndex);

        List<Vector3> pathInCluster = thetaStarPathfinder.FindPathInClusterList(entrancePosition, goalPosition, data.ClusterIndexes);

        for (int i = 0; i < data.ClusterIndexes.Count; i++)
        {
            clusterList.SetClusterActive(data.ClusterIndexes[i], false);
        }

        return pathInCluster;
    }
}
