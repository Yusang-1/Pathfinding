using UnityEngine;
using System;

public class UIFindAllPath : MonoBehaviour
{
    public event Action OnFindAllPathEvent;
    private Action findAllPath;
    
    public void Initialize(Action findAllPath)
    {
        this.findAllPath = findAllPath;
    }
    
    public void OnFindAllPath()
    {
        findAllPath?.Invoke();
        gameObject.SetActive(false);
        OnFindAllPathEvent?.Invoke();
    }
}
