using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class SearchWithTheClusterResult
    {
        private readonly ThetaStar thetaStarPathfinder;
        public SearchWithTheClusterResult(ThetaStar thetaStarPathfinder)
        {
            this.thetaStarPathfinder = thetaStarPathfinder;
        }

        public List<Vector3> FindPathThetaWithClusterList(ClusterSmootherResult data, NodeList nodeList, HPAClusterList clusterList, float unitRadius)
        {
            for (int i = 0; i < data.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(data.ClusterIndexes[i], true);
            }

            Vector3 entrancePosition = nodeList.GridToWorld(data.EnterNodeIndex);
            Vector3 goalPosition = nodeList.GridToWorld(data.ExitNodeIndex);

            List<Vector3> pathInCluster = thetaStarPathfinder.FindThetaPathInClusterList(entrancePosition, goalPosition, data.ClusterIndexes, unitRadius);

            for (int i = 0; i < data.ClusterIndexes.Count; i++)
            {
                clusterList.SetClusterActive(data.ClusterIndexes[i], false);
            }

            return pathInCluster;
        }
    }
}
