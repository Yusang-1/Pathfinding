using System;
using System.Collections.Generic;

public class ObjectPool<TObject> where TObject : class, IPoolObject<TObject>
{
    private readonly Stack<TObject> stackNotUsed = new();
    private readonly HashSet<TObject> stackCurrentUsed = new();
    
    public void PoolObjectUnused(TObject tObject)
    {
        if(stackCurrentUsed.Contains(tObject))
        {
            stackNotUsed.Push(tObject);
            stackCurrentUsed.Remove(tObject);
        }
    }
    
    public void PoolObjectFirstCreated(TObject tObject)
    {
        stackCurrentUsed.Add(tObject);
    }
        
    public bool TryGetObject(out TObject tObject)
    {
        if (stackNotUsed.Count > 0)
        {
            tObject = stackNotUsed.Pop();
            stackCurrentUsed.Add(tObject);
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
    public event Action<TObject> OnPoolObjectFirstCreated;
    public event Action<TObject> OnPoolObjectUnused;
}
