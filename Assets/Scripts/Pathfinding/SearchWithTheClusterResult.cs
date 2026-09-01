using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.Pathfinding
{
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

        public List<Vector3> FindPath(ClusterResultWrapper resultWrapper)
        {
            resultPath.Clear();
            PathResultRecorder.ResetPathLength();

            List<ClusterSmootherResult> smoothPathData = resultWrapper.ClusterSmootherResult;
            foreach (var path in smoothPathData)
            {
                Vector3 entrancePosition = nodeList.GridToWorld(path.EnterNodeIndex);
                Vector3 goalPosition = nodeList.GridToWorld(path.ExitNodeIndex);

                aStarPathfinder.SetGetNeighborPolicy(new GetNeighborNodesWithClusterListProvider(nodeList, clusterList));
                (aStarPathfinder.GetNeighborNodesActionProvider as GetNeighborNodesWithClusterListProvider).SetClusterList(path.ClusterIndexes);
                List<Vector3> pathInCluster = aStarPathfinder.FindPath(entrancePosition, goalPosition, 0);

                PathResultRecorder.AddPathLength(1); // cluster이동 비용 1;

                resultPath.AddRange(pathInCluster);
                Vector3ListPool.ReleaseValue(pathInCluster);
            }

            PathResultRecorder.AddPathLength(-1); // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
            return resultPath;
        }

        public List<Vector3> FindSmoothPathTheta(ClusterResultWrapper resultWrapper)
        {
            resultPath.Clear();
            PathResultRecorder.ResetPathLength();

            List<ClusterSmootherResult> smoothPathData = resultWrapper.ClusterSmootherResult;
            foreach (var path in smoothPathData)
            {
                Vector3 entrancePosition = nodeList.GridToWorld(path.EnterNodeIndex);
                Vector3 goalPosition = nodeList.GridToWorld(path.ExitNodeIndex);

                thetaStarPathfinder.SetGetNeighborPolicy(new GetNeighborNodesWithClusterListProvider(nodeList, clusterList));
                (thetaStarPathfinder.GetNeighborNodesActionProvider as GetNeighborNodesWithClusterListProvider).SetClusterList(path.ClusterIndexes);
                List<Vector3> pathInCluster = thetaStarPathfinder.FindPath(entrancePosition, goalPosition, 0);

                PathResultRecorder.AddPathLength(1); // cluster이동 비용 1;

                if (pathInCluster == null) continue;
                resultPath.AddRange(pathInCluster);
                Vector3ListPool.ReleaseValue(pathInCluster);
            }

            PathResultRecorder.AddPathLength(-1); // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
            return resultPath;
        }

        public List<Vector3> FindPathTheta(ClusterResultWrapper resultWrapper)
        {
            resultPath.Clear();
            PathResultRecorder.ResetPathLength();

            // thetaStarPathfinder에 이웃 탐색 정책 설정
            thetaStarPathfinder.SetGetNeighborPolicy(new GetNeighborNodesInSameClusterProvider(nodeList, clusterList));

            List<ClusterSmootherResult> smoothPathData = resultWrapper.ClusterSmootherResult;
            foreach (var path in smoothPathData)
            {
                foreach (var index in path.ClusterIndexes)
                {
                    clusterList.SetClusterActive(index, true);
                }

                Vector3 entrancePosition = nodeList.GridToWorld(path.EnterNodeIndex);
                Vector3 goalPosition = nodeList.GridToWorld(path.ExitNodeIndex);

                List<Vector3> pathInCluster = thetaStarPathfinder.FindPath(entrancePosition, goalPosition, 0);

                PathResultRecorder.AddPathLength(1); // cluster이동 비용 1;

                if (pathInCluster == null) continue;
                resultPath.AddRange(pathInCluster);
                Vector3ListPool.ReleaseValue(pathInCluster);

                foreach (var index in path.ClusterIndexes)
                {
                    clusterList.SetClusterActive(index, false);
                }
            }

            PathResultRecorder.AddPathLength(-1); // 마지막 cluster에서는 이동하지 않으므로 비용 -1;
            return resultPath;
        }

        public List<Vector3> FindPathThetaWithClusterList(ClusterSmootherResult smoothPath, float unitRadius)
        {                        
            return Find(smoothPath.ClusterIndexes, smoothPath.EnterNodeIndex, smoothPath.ExitNodeIndex, unitRadius);
        }
        
        public List<Vector3> FindPathThetaWithClusterList(List<Vector2Int> clusterIndexes, Vector2Int enterNode, Vector2Int exitNode, float unitRadius)
        {
            return Find(clusterIndexes, enterNode, exitNode, unitRadius);
        }
        
        private List<Vector3> Find(List<Vector2Int> clusterIndexes, Vector2Int enterNode, Vector2Int exitNode, float unitRadius)
        {
            Vector3 entrancePosition = nodeList.GridToWorld(enterNode);
            Vector3 goalPosition = nodeList.GridToWorld(exitNode);

            thetaStarPathfinder.SetGetNeighborPolicy(new GetNeighborNodesWithClusterListProvider(nodeList, clusterList));
            (thetaStarPathfinder.GetNeighborNodesActionProvider as GetNeighborNodesWithClusterListProvider).SetClusterList(clusterIndexes);
            List<Vector3> pathInCluster = thetaStarPathfinder.FindPath(entrancePosition, goalPosition, unitRadius);

            return pathInCluster;
        }
    }
}
