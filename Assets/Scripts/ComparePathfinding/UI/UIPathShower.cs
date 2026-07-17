using UnityEngine;
using System;

public class UIPathShower : MonoBehaviour
{
    public event Action OnShowAStarPathRequested;
    public event Action OnShowHAPStarPathRequested;
    public event Action OnShowHPAThetaPathRequested;
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
    
    public void OnShowHPAThetaPathButton()
    {
        OnShowHPAThetaPathRequested?.Invoke();
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
    
    
    private bool beforeActiveStatus;
    public void SetTempActiveFalse()
    {
        beforeActiveStatus = gameObject.activeSelf;
        gameObject.SetActive(false);
    }

    public void ResetToBeforeActiveStatus()
    {
        gameObject.SetActive(beforeActiveStatus);
    }
}
