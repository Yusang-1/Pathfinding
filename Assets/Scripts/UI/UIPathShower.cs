using UnityEngine;
using System;

public class UIPathShower : MonoBehaviour
{
    private Action resetAll;
    private Action<bool> activeResultController;
    
    public void Initialize(Action resetAll, Action<bool> activeResultController)
    {
        this.resetAll = resetAll;
        this.activeResultController = activeResultController;
    }
    
    public void OnResetAll()
    {
        resetAll?.Invoke();
        gameObject.SetActive(false);
        activeResultController?.Invoke(false);
    }
}
