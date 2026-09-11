using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.Pathfinding
{
    public class PathCacheContainer
    {
        private readonly Dictionary<ClusterSmootherResult, List<Vector3>> cachedPath = new();

        public bool TryGetCachedPath(ClusterSmootherResult clusterResult, out List<Vector3> result)
        {
            if (cachedPath.ContainsKey(clusterResult))
            {
                result = cachedPath[clusterResult];
                Debug.Log("Get Cached Path");
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public void SetCachedPath(ClusterSmootherResult clusterResult, List<Vector3> result)
        {
            List<Vector3> list = Vector3ListPool.GetValue();
            list.AddRange(result);

            if (!cachedPath.ContainsKey(clusterResult))
            {
                cachedPath.Add(clusterResult, list);
            }
        }

        public void Clear()
        {
            foreach (var values in cachedPath.Values)
            {
                Vector3ListPool.ReleaseValue(values);
            }
            cachedPath.Clear();
        }
    }
}
