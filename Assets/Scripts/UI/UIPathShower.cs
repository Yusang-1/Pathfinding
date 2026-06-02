using UnityEngine;
using System;

public class UIPathShower : MonoBehaviour
{
    private Action resetAll;
    public void Initialize(Action resetAll)
    {
        this.resetAll = resetAll;
    }
    
    public void OnResetAll()
    {
        resetAll?.Invoke();
        gameObject.SetActive(false);
    }
}
