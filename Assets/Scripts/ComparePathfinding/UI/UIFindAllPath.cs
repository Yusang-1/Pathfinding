using UnityEngine;
using System;

public class UIFindAllPath : MonoBehaviour
{
    public event Action OnFindAllPathEvent;
    
    public void OnFindAllPath()
    {
        gameObject.SetActive(false);
        OnFindAllPathEvent?.Invoke();
    }
}
