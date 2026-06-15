using System;
using UnityEngine;

public class Cluster : MonoBehaviour, IPoolObject<Cluster>
{
    public event Action<Cluster> OnPoolObjectFirstCreated;
    public event Action<Cluster> OnPoolObjectUnused;


    public void Initialize()
    {
        OnPoolObjectFirstCreated?.Invoke(this);
    }
    
    public void ResetCluster()
    {
        OnPoolObjectUnused?.Invoke(this);
    }
}
