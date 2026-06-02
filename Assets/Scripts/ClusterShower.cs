using System.Collections.Generic;
using UnityEngine;

public class ClusterShower : MonoBehaviour
{
    [SerializeField] private GameObject clusterPrefab;
    private readonly Dictionary<Vector2Int, GameObject> clusters = new();
    private int clusterSize;
    
    public void Initialize(int clusterSize)
    {
        this.clusterSize = clusterSize;
    }
    
    public void ShowCluster(HPAClusterList clusterList)
    {
        for (int i = 0; i < clusterSize; i++)
        {
            for (int j = 0; j < clusterSize; j++)
            {
                GameObject go = Instantiate(clusterPrefab, new Vector2(2 + i * clusterSize, 2 + j * clusterSize), Quaternion.identity);
                go.SetActive(false);
                clusters.Add(new Vector2Int(i, j), go);
            }
        }
    }
    
    public void ShowActivatedClusters(List<Vector2Int> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            clusters[list[i]].SetActive(true);
        }
    }
    public void ShowActivatedClusters(List<HPAPathfinder.ResultNode> results)
    {
        for(int i = 0; i < results.Count; i++)
        {
            clusters[results[i].ClusterIndex].SetActive(true);
        }
    }
    
    public void ShowActivated(Vector2Int index)
    {
        clusters[index].SetActive(true);
    }
    
    public void DeactiveClusters()
    {
        foreach(var item in clusters)
        {
            item.Value.SetActive(false);
        }
    }
    
    public void ResetClusters()
    {
        DeactiveClusters();
        clusters.Clear();
    }
}
