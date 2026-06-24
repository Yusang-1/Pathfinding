using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class LazyRefine
    {
        private readonly Queue<Vector3> pathQueue = new();

        private readonly HPAClusterList clusterList;
        private readonly SearchWithTheClusterResult searchWithTheClusterResult;
        private readonly NodeList nodeList;
        public LazyRefine(HPAClusterList clusterList, NodeList nodeList, SearchWithTheClusterResult searchWithTheClusterResult)
        {
            this.clusterList = clusterList;
            this.nodeList = nodeList;
            this.searchWithTheClusterResult = searchWithTheClusterResult;
        }

        public bool TryGetPathFromQueue(out Vector3 path)
        {
            if (pathQueue.Count <= 0)
            {
                path = Vector3.zero;
                return false;
            }
            else
            {
                path = pathQueue.Dequeue();
                return true;
            }
        }

        public void DoLazyRefinement(HPAPathfinder.ResultNode result, LineDrawer lineDrawer, float unitRadius)
        {
            List<Vector3> resultPath = searchWithTheClusterResult.FindPathTheta(result, nodeList, clusterList, unitRadius);

            lineDrawer.DrawLine(resultPath);

            for (int i = 0; i < resultPath.Count; i++)
            {
                pathQueue.Enqueue(resultPath[i]);
            }
        }

        /// <summary> 하나의 cluster ResultNode의 경로를 PathQueue에 담는다. </summary>
        public void DoLazyRefinement(HPAPathfinder.ResultNode result, bool isEnd, Vector3 finalDestination, bool isFirst, Vector3 startPosition, float unitRadius)
        {
            List<Vector3> resultPath = searchWithTheClusterResult.FindPathTheta(result, nodeList, clusterList, unitRadius);
            if (resultPath == null) return;

            if (isFirst)
            {
                resultPath[0] = startPosition;
            }
            if (isEnd)
            {
                resultPath[^1] = finalDestination;
            }

            for (int i = 0; i < resultPath.Count; i++)
            {
                pathQueue.Enqueue(resultPath[i]);
            }
        }

        public void ResetLazyRefine()
        {
            pathQueue.Clear();
        }
    }
}
