using UnityEngine;
using System.Collections.Generic;

public class ClusterShower : MonoBehaviour
{
    [SerializeField] private Cluster clusterPrefab;
    private readonly Dictionary<Vector2Int, Cluster> clusters = new();

    private readonly ObjectPool<Cluster> clusterPool = new();

    public void Initialize(int clusterCount, int clusterSize, int nodeSize)
    {
        CreateClusterImage(clusterCount, clusterSize, nodeSize);
    }

    private void CreateClusterImage(int clusterCount, int clusterSize, int nodeSize)
    {
        for (int i = 0; i < clusterCount; i++)
        {
            for (int j = 0; j < clusterCount; j++)
            {
                if (!clusterPool.TryGetObject(out Cluster cluster))
                {
                    // pool에서 찾지 못한 경우
                    cluster = Instantiate(clusterPrefab, new Vector2((float)clusterSize / 2 - (float)nodeSize / 2 + i * clusterSize, (float)clusterSize / 2 - (float)nodeSize / 2 + j * clusterSize), Quaternion.identity);
                    cluster.OnPoolObjectFirstCreated += clusterPool.PoolObjectFirstCreated;
                    cluster.OnPoolObjectUnused += clusterPool.PoolObjectUnused;
                    cluster.Initialize();
                }
                else
                {
                    cluster.transform.position = new Vector2((float)clusterSize / 2 - (float)nodeSize / 2 + i * clusterSize, (float)clusterSize / 2 - (float)nodeSize / 2 + j * clusterSize);
                }
                
                cluster.transform.localScale = new Vector3(clusterSize, clusterSize, 1);
                cluster.gameObject.SetActive(false);
                clusters.Add(new Vector2Int(i, j), cluster);
            }
        }
    }

    public void ShowActivatedClusters(List<ClusterSmootherResult> results)
    {
        ClusterSmootherResult result;
        for (int i = 0; i < results.Count; i++)
        {
            result = results[i];
            for(int j = 0; j < result.ClusterIndexes.Count; j++)
            {
                clusters[result.ClusterIndexes[j]].gameObject.SetActive(true);                
            }
        }
    }

    public void ResetClusters()
    {
        foreach (var item in clusters)
        {
            item.Value.gameObject.SetActive(false);
        }
    }
    public void ResetAllClusters()
    {
        foreach (var cluster in clusters.Values)
        {
            cluster.gameObject.SetActive(false);
            cluster.ResetCluster();
        }
        
        clusters.Clear();
    }
}
