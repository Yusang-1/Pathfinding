using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
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

        public List<Vector3> FindPath(List<HPAPathfinder.ResultNode> pathData, NodeList nodeList, HPAClusterList clusterList, out PathResult pathResult)
        {
            pathResult = new();
            resultPath.Clear();
            foreach (var data in pathData)
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

                List<Vector3> pathInCluster = aStarPathfinder.FindPathInSameCluster(entrancePosition, goalPosition, out PathResult result);
                pathResult.AddResult(result);
                pathResult.PathLength++; // cluster이동 비용 1;            

                resultPath.AddRange(pathInCluster);

                clusterList.SetClusterActive(data.Index, false);
            }

            pathResult.PathLength--; // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
            return resultPath;
        }

        public List<Vector3> FindPathTheta(List<HPAPathfinder.ResultNode> pathData, NodeList nodeList, HPAClusterList clusterList, out PathResult pathResult)
        {
            resultPath.Clear();
            pathResult = new();
            foreach (var data in pathData)
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

                List<Vector3> pathInCluster = thetaStarPathfinder.FindPath(entrancePosition, goalPosition, out PathResult result);
                pathResult.AddResult(result);
                pathResult.PathLength++; // cluster이동 비용 1;

                if (pathInCluster == null) continue;
                resultPath.AddRange(pathInCluster);

                clusterList.SetClusterActive(data.Index, false);
            }

            pathResult.PathLength--; // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
            return resultPath;
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
