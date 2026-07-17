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
