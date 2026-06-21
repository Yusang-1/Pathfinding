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

        public List<Vector3> FindPathTheta(HPAPathfinder.ResultNode data, NodeList nodeList, HPAClusterList clusterList)
        {
            clusterList.SetClusterActive(data.Index, true);

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

            List<Vector3> pathInCluster = thetaStarPathfinder.FindPath(entrancePosition, goalPosition, out PathResult pathResult);

            clusterList.SetClusterActive(data.Index, false);

            return pathInCluster;
        }
    }
}
