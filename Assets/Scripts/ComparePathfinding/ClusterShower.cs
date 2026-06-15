using System.Collections.Generic;
using UnityEngine;

public class ClusterShower : MonoBehaviour
{
    [SerializeField] private GameObject clusterPrefab;
    private readonly Dictionary<Vector2Int, GameObject> clusters = new();
    
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
                GameObject go = Instantiate(clusterPrefab, new Vector2((float)clusterSize/2 - (float)nodeSize/2 + i * clusterSize, (float)clusterSize/2 - (float)nodeSize/2  + j * clusterSize), Quaternion.identity);
                go.transform.localScale = new Vector3(clusterSize, clusterSize, 1);
                go.SetActive(false);
                clusters.Add(new Vector2Int(i, j), go);
            }
        }
    }

    public void ShowActivatedClusters(List<HPAPathfinder.ResultNode> results)
    {
        for(int i = 0; i < results.Count; i++)
        {
            clusters[results[i].Index].SetActive(true);
        }
    }
    
    public void ResetClusters()
    {
        foreach(var item in clusters)
        {
            item.Value.SetActive(false);
        }        
    }
    public void ResetAllClusters() => clusters.Clear();
}
