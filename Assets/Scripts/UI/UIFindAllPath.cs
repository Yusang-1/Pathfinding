using UnityEngine;
using System;

public class UIFindAllPath : MonoBehaviour
{
    public event Action OnFindAllPathEvent;
    private Action findAllPath;
    private Action<bool> activeResultController;
    
    public void Initialize(Action findAllPath, Action<bool> activeResultController)
    {
        this.findAllPath = findAllPath;
        this.activeResultController = activeResultController;
    }
    
    public void OnFindAllPath()
    {
        findAllPath?.Invoke();
        gameObject.SetActive(false);
        OnFindAllPathEvent?.Invoke();
        activeResultController?.Invoke(true);
    }
}
