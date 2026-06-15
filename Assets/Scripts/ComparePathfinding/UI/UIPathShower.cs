using UnityEngine;
using System;

public class UIPathShower : MonoBehaviour
{
    public event Action OnResetAll;
    
    public void ResetAll()
    {
        gameObject.SetActive(false);
        OnResetAll?.Invoke();
    }
}
