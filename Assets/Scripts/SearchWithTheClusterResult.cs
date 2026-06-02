using UnityEngine;
using System.Collections.Generic;

public class SearchWithTheClusterResult
{
    private readonly List<Vector3> resultPath = new();

    public List<Vector3> FindPath(List<HPAPathfinder.ResultNode> pathData, AStarPathfinder pathfinder, NodeList nodeList, HPAClusterList clusterList)
    {
        resultPath.Clear();
        foreach (var data in pathData)
        {
            clusterList.SetClusterActive(data.ClusterIndex, true);

            Vector3 entrancePosition = nodeList.GridToWorld(data.enteranceNode);
            Vector3 goalPosition = nodeList.GridToWorld(data.exitNode);
            List<Vector3> pathInCluster = pathfinder.FindPath(entrancePosition, goalPosition);
            resultPath.AddRange(pathInCluster);

            clusterList.SetClusterActive(data.ClusterIndex, false);
        }

        return resultPath;
    }

    public List<Vector3> FindPathTheta(List<HPAPathfinder.ResultNode> pathData, ThetaStar pathfinder, NodeList nodeList, HPAClusterList clusterList)
    {
        resultPath.Clear();
        foreach (var data in pathData)
        {
            clusterList.SetClusterActive(data.ClusterIndex, true);

            Vector3 entrancePosition, goalPosition;
            if (data.hasEntranceAndExit == false)
            {
                entrancePosition = nodeList.GridToWorld(data.exitNode);
                goalPosition = nodeList.GridToWorld(data.exitNode);
            }
            else
            {
                entrancePosition = nodeList.GridToWorld(data.enteranceNode);
                goalPosition = nodeList.GridToWorld(data.exitNode);
            }

            List<Vector3> pathInCluster = pathfinder.FindPath(entrancePosition, goalPosition);
            if(pathInCluster == null) continue;
            resultPath.AddRange(pathInCluster);

            clusterList.SetClusterActive(data.ClusterIndex, false);
        }

        return resultPath;
    }

    public List<Vector3> FindPathTheta(HPAPathfinder.ResultNode data, ThetaStar pathfinder, NodeList nodeList, HPAClusterList clusterList)
    {
        clusterList.SetClusterActive(data.ClusterIndex, true);

        Vector3 entrancePosition, goalPosition;
        if (data.hasEntranceAndExit == false)
        {
            entrancePosition = nodeList.GridToWorld(data.exitNode);
            goalPosition = nodeList.GridToWorld(data.exitNode);
        }
        else
        {
            entrancePosition = nodeList.GridToWorld(data.enteranceNode);
            goalPosition = nodeList.GridToWorld(data.exitNode);
        }

        List<Vector3> pathInCluster = pathfinder.FindPath(entrancePosition, goalPosition);        

        clusterList.SetClusterActive(data.ClusterIndex, false);
        
        return pathInCluster;
    }
}
