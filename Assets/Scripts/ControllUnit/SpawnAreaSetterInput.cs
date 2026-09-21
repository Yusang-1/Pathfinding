using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class SpawnAreaSetterInput : MonoBehaviour, IActionMapInputer
{
    public event Action OnSetSpawnAreaFinished;
    public event Action OnSpawnUnitRequested;
    public event Action<Vector3> OnTrackMouse;
    public event Action OnCancelSpawnAreaSet;
    public event Action<bool> OnPointerNotOverGameObject;

    [SerializeField] private ActionMaps actionMap;

    private Vector2 mouseWorldPosition;
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

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (isPointerOverGameObject || !isInputActive) return;

        if (context.canceled)
        {
            OnSpawnUnitRequested?.Invoke();
            OnSetSpawnAreaFinished?.Invoke();
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (isPointerOverGameObject || !isInputActive) return;

        if (context.canceled)
        {
            OnCancelSpawnAreaSet?.Invoke();
            OnSetSpawnAreaFinished?.Invoke();
        }
    }

    public void OnTrackMousePosition(InputAction.CallbackContext context)
    {
        if (!isInputActive) return;

        if (context.performed)
        {
            OnPointerNotOverGameObject?.Invoke(!isPointerOverGameObject);

            Vector2 mousePosition = context.ReadValue<Vector2>();
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));

            OnTrackMouse?.Invoke(mouseWorldPosition);
        }
    }

    public void ActionMapActivated() => isInputActive = true;

    public void ActionMapDeactivated() => isInputActive = false;

    public ActionMaps GetActionMap() => actionMap;
}
