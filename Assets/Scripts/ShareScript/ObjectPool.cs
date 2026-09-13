using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<TObject> where TObject : MonoBehaviour
{
    private readonly Stack<TObject> stackNotUsed = new();
    
    private const int MAX_POOL_SIZE = 32;
    
    public void PoolObjectUnused(TObject tObject)
    {
        if(stackNotUsed.Count < MAX_POOL_SIZE)
        {
            stackNotUsed.Push(tObject);            
        }
        else
        {
            GameObject.Destroy(tObject);
        }
    }
        
    public bool TryGetObject(out TObject tObject)
    {
        if (stackNotUsed.Count > 0)
        {
            tObject = stackNotUsed.Pop();
            return true;
        }
        else
        {
            tObject = null;
            return false;
        }
    }
}

public interface IPoolObject<TObject> where TObject : IPoolObject<TObject>
{
    public event Action<TObject> OnPoolObjectUnused;
}
