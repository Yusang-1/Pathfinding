using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class Pathfinder : MonoBehaviour
    {
        private NodeList nodeList;
        private HPAClusterList clusterList;

        private HPAPathfinder highLevelPathfinder;
        private ClusterPathSmoother clusterPathSmoother;
        private readonly ClusterResultWrapper clusterResultWrapper = new();

        public void SetNodeAndCluster(NodeList nodes, in MapData mapData, Dictionary<UnitSize, float> unitRadiusList)
        {
            nodeList = nodes;
            clusterList = new HPAClusterList(nodeList);

            AStarPathfinder aStarPathfinder = new(nodeList);

            clusterList.Initialize(aStarPathfinder, mapData.MapSize, mapData.ClusterSize, unitRadiusList);
            nodeList.SetNodeArea();
            
            clusterPathSmoother = new ClusterPathSmoother(nodeList, clusterList);
            highLevelPathfinder = new HPAPathfinder(nodeList, clusterList);
        }

        public LazyRefine GetLazyRefine()
        {
            ThetaStar thetaStarPathfinder = new(nodeList);

            return new LazyRefine(clusterList, nodeList, new SearchWithTheClusterResult(thetaStarPathfinder));
        }

        public ClusterResultWrapper GetAbstractPath(Vector3 from, Vector3 to, float unitRadius)
        {
            clusterResultWrapper.Reset();
            clusterResultWrapper.SetStart(from, to, unitRadius);

            var clusterPath = highLevelPathfinder.FindClusterPath(clusterResultWrapper);
            var smootherClusterPath = clusterPathSmoother.SmoothClusterPath(clusterPath);
            return smootherClusterPath;
        }
    }
}
