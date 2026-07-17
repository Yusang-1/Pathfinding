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

    public List<Vector3> FindPath(List<ClusterResult> pathData)
    {
        resultPath.Clear();
        PathResultRecorder.ResetPathLength();
        
        foreach (var data in pathData)
        {
            var path = data.GetSmoothClusterPath();
            
            for (int i = 0; i < path.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(path.ClusterIndexes[i], true);
            }

            Vector3 entrancePosition = nodeList.GridToWorld(path.EnterNodeIndex);
            Vector3 goalPosition = nodeList.GridToWorld(path.ExitNodeIndex);

            List<Vector3> pathInCluster = aStarPathfinder.FindPathInClusterList(entrancePosition, goalPosition, path.ClusterIndexes);

            PathResultRecorder.AddPathLength(1); // cluster이동 비용 1;

            resultPath.AddRange(pathInCluster);
            Vector3ListPool.ReleaseValue(pathInCluster);

            for (int i = 0; i < path.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(path.ClusterIndexes[i], false);
            }
        }

        PathResultRecorder.AddPathLength(-1); // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
        return resultPath;
    }

    public List<Vector3> FindSmoothPathTheta(List<ClusterResult> pathData)
    {
        resultPath.Clear();
        PathResultRecorder.ResetPathLength();
        
        foreach (var data in pathData)
        {
            var path = data.GetSmoothClusterPath();
            
            for (int i = 0; i < path.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(path.ClusterIndexes[i], true);
            }

            Vector3 entrancePosition = nodeList.GridToWorld(path.EnterNodeIndex);
            Vector3 goalPosition = nodeList.GridToWorld(path.ExitNodeIndex);

            List<Vector3> pathInCluster = thetaStarPathfinder.FindPathInClusterList(entrancePosition, goalPosition, path.ClusterIndexes);

            PathResultRecorder.AddPathLength(1); // cluster이동 비용 1;

            if (pathInCluster == null) continue;
            resultPath.AddRange(pathInCluster);
            Vector3ListPool.ReleaseValue(pathInCluster);

            for (int i = 0; i < path.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(path.ClusterIndexes[i], false);
            }
        }

        PathResultRecorder.AddPathLength(-1); // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
        return resultPath;
    }
    
    public List<Vector3> FindPathTheta(List<ClusterResult> pathData)
    {
        resultPath.Clear();
        PathResultRecorder.ResetPathLength();
        
        foreach (var data in pathData)
        {
            var path = data.GetClusterResult();
            
            clusterList.SetClusterActive(path.Index, true);

            Vector3 entrancePosition = nodeList.GridToWorld(path.EntranceEnter);
            Vector3 goalPosition = nodeList.GridToWorld(path.EntranceExit);

            List<Vector3> pathInCluster = thetaStarPathfinder.FindPath(entrancePosition, goalPosition);

            PathResultRecorder.AddPathLength(1); // cluster이동 비용 1;

            if (pathInCluster == null) continue;
            resultPath.AddRange(pathInCluster);
            Vector3ListPool.ReleaseValue(pathInCluster);

            clusterList.SetClusterActive(path.Index, false);        
        }

        PathResultRecorder.AddPathLength(-1); // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
        return resultPath;
    }

    public List<Vector3> FindPathThetaWithClusterList(ClusterResult data, NodeList nodeList, HPAClusterList clusterList)
    {
        var path = data.GetSmoothClusterPath();
        
        for (int i = 0; i < path.ClusterIndexes.Count; i++)
        {
            clusterList.SetClusterActive(path.ClusterIndexes[i], true);
        }

        Vector3 entrancePosition = nodeList.GridToWorld(path.EnterNodeIndex);
        Vector3 goalPosition = nodeList.GridToWorld(path.ExitNodeIndex);

        List<Vector3> pathInCluster = thetaStarPathfinder.FindPathInClusterList(entrancePosition, goalPosition, path.ClusterIndexes);

        for (int i = 0; i < path.ClusterIndexes.Count; i++)
        {
            clusterList.SetClusterActive(path.ClusterIndexes[i], false);
        }

        return pathInCluster;
    }
}
