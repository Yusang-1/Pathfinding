using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class Pathfinder : MonoBehaviour
    {
        private NodeList nodeList;
        private HPAClusterList clusterList;

        private HPAPathfinder highLevelPathfinder;

        public void SetNodeAndCluster(NodeList nodes, int mapSize, int clusterSize, Dictionary<UnitSize, float> unitRadiusList)
        {
            nodeList = nodes;
            clusterList = new HPAClusterList(nodeList);
            
            AStarPathfinder aStarPathfinder = new(nodeList, clusterList);

            clusterList.Initialize(aStarPathfinder, mapSize, clusterSize, unitRadiusList);
            nodeList.SetNodeArea();

            highLevelPathfinder = new HPAPathfinder(clusterList, nodeList);
        }
        
        public LazyRefine GetLazyRefine()
        {
            ThetaStar thetaStarPathfinder = new(nodeList, clusterList);
            
            return new LazyRefine(clusterList, nodeList, new SearchWithTheClusterResult(thetaStarPathfinder));
        }

        public List<HPAPathfinder.ResultNode> GetAbstractPath(Vector3 from, Vector3 to, float unitRadius)
        {
            return highLevelPathfinder.FindClusterPath(from, to, out PathResult result, unitRadius);
        }        
    }
}
