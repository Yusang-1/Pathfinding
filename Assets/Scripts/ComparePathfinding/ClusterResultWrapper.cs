using System.Collections.Generic;
using UnityEngine;

public class ClusterResultWrapper
{
    public Vector3 From { get; private set; }
    public Vector3 To { get; private set; }
    public float UnitRadius { get; private set; }

    public List<ClusterSmootherResult> ClusterSmootherResult { get; private set; } = new();
    public List<NewClusterResult> NewClusterResults { get; private set; } = new();

    public void SetStart(Vector3 from, Vector3 to, float unitRadius)
    {
        this.From = from;
        this.To = to;
        this.UnitRadius = unitRadius;
    }
    
    public void Reset()
    {
        
    }
    
    public void SetClusterResult(List<NewClusterResult> results)
    {
        NewClusterResults = results;
    }
    public void SetClusterResult(NewClusterResult result)
    {
        NewClusterResults.Add(result);
    }
    
    public void SetClusterSmootherResult(List<ClusterSmootherResult> smootherResults)
    {
        ClusterSmootherResult = smootherResults;
    }
    public void SetClusterSmootherResult(ClusterSmootherResult smootherResult)
    {
        ClusterSmootherResult.Add(smootherResult);
    }
}
