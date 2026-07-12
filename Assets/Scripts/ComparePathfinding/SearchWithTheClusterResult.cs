using UnityEngine;
using System.Collections.Generic;

public class SearchWithTheClusterResult
{
    private readonly List<Vector3> resultPath = new();

    private readonly AStarPathfinder aStarPathfinder;
    private readonly ThetaStar thetaStarPathfinder;
    public SearchWithTheClusterResult(AStarPathfinder aStarPathfinder, ThetaStar thetaStarPathfinder)
    {
        this.aStarPathfinder = aStarPathfinder;
        this.thetaStarPathfinder = thetaStarPathfinder;
    }

    public List<Vector3> FindPath(List<ClusterSmootherResult> pathData, NodeList nodeList, HPAClusterList clusterList)
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

            for (int i = 0; i < data.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(data.ClusterIndexes[i], false);
            }
        }

        PathResultRecorder.AddPathLength(-1); // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
        return resultPath;
    }

    public List<Vector3> FindPathTheta(List<ClusterSmootherResult> pathData, NodeList nodeList, HPAClusterList clusterList)
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
