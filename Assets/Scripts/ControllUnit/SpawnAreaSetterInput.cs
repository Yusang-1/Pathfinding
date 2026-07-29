using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class SpawnAreaSetterInput : MonoBehaviour, IActionMapInputer
{
    public event Action<Vector3> OnSetSpawnAreaRequested;
    public event Action OnSetSpawnAreaFinished;

    [SerializeField] private string actionMapName;

    private Vector2 mousePosition;
    private bool isPointerOverGameObject;
    private bool isInputActive;

    private void Update()
    {
        if (!isInputActive) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            isPointerOverGameObject = true;
        }
        else
        {
            isPointerOverGameObject = false;
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (isPointerOverGameObject || !isInputActive) return;

        if (context.canceled)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
            OnSetSpawnAreaRequested?.Invoke(worldPos);
            OnSetSpawnAreaFinished?.Invoke();
        }
    }

    public void OnTrackMousePosition(InputAction.CallbackContext context)
    {
        if (!isInputActive) return;

        if (context.performed)
        {
            mousePosition = context.ReadValue<Vector2>();
        }
    }

    public void ActionMapActivated() => isInputActive = true;

    public void ActionMapDeactivated() => isInputActive = false;

    public string GetActionMapName() => actionMapName;
}
