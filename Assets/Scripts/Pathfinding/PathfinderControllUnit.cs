using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.Pathfinding
{
    public class PathfinderControllUnit : MonoBehaviour
    {
        private NodeList nodeList;
        private HPAClusterList clusterList;
        
        private AStarPathfinder aStarPathfinder;
        private HPAPathfinder highLevelPathfinder;
        private ClusterPathSmoother clusterPathSmoother;
        private readonly ClusterResultWrapper clusterResultWrapper = new();

        public void SetNodeAndCluster(NodeList nodes, int mapSize, int clusterSize, Dictionary<UnitSize, float> unitRadiusList)
        {
            nodeList = nodes;
            clusterList = new HPAClusterList(nodeList);

            aStarPathfinder = new(nodeList);

            clusterList.Initialize(aStarPathfinder, mapSize, clusterSize, unitRadiusList);
            nodeList.SetNodeArea();
            
            clusterPathSmoother = new ClusterPathSmoother(nodeList, clusterList);
            highLevelPathfinder = new HPAPathfinder(nodeList, clusterList);
        }

        public LazyRefine GetLazyRefine()
        {
            ThetaStar thetaStarPathfinder = new(nodeList);
            
            var searchWithTheClusterResult = new SearchWithTheClusterResult(aStarPathfinder, thetaStarPathfinder, clusterList, nodeList);
            return new LazyRefine(searchWithTheClusterResult);
        }

        public ClusterResultWrapper GetAbstractPath(Vector3 from, Vector3 to, float unitRadius)
        {
            clusterResultWrapper.ResetClusterResult();
            clusterResultWrapper.SetStart(from, to, unitRadius);

            var clusterPath = highLevelPathfinder.FindClusterPath(clusterResultWrapper);
            var smootherClusterPath = clusterPathSmoother.SmoothClusterPath(clusterPath);
            return smootherClusterPath;
        }
    }
}
