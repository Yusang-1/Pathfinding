using UnityEngine;
using System;
using TMPro;

public class UIPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI popupText;
    
    public void Initialize()
    {
        gameObject.SetActive(true);
        PopupService.OnPopupRequested += ShowPopup;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        PopupService.OnPopupRequested -= ShowPopup;
    }

    public void ShowPopup(string text)
    {
        popupText.text = text;
        gameObject.SetActive(true);
    }
    
    public void SetActiveFalse() => gameObject.SetActive(false);
}

public static class PopupService
{
    public static event Action<string> OnPopupRequested;

    public static void Show(string message)
    {
        OnPopupRequested?.Invoke(message);
    }
}
