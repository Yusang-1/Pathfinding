using UnityEngine;
using System;

public class UIPathShower : MonoBehaviour
{
    public event Action OnShowAStarPathRequested;
    public event Action OnShowHAPStarPathRequested;
    public event Action OnShowHAPStarSmoothingPathRequested;
    public event Action OnResetAllRequested;
    public event Action OnShowMoveUnitRequested;
    
    public void OnShowAStarPathButton()
    {
        OnShowAStarPathRequested?.Invoke();
    }
    
    public void OnShowHAPStarPathButton()
    {
        OnShowHAPStarPathRequested?.Invoke();
    }
    
    public void OnShowHAPStarSmoothingPathButton()
    {
        OnShowHAPStarSmoothingPathRequested?.Invoke();
    }
    
    public void OnResetAllButton()
    {
        gameObject.SetActive(false);
        OnResetAllRequested?.Invoke();
    }
    
    public void OnShowMoveUnitButton()
    {
        OnShowMoveUnitRequested?.Invoke();
    }
}
