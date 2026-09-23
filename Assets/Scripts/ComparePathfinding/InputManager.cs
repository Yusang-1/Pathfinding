using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public event Action OnControllMenu;

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private PlayerControllInput playerControllInput;    
    private InputActionMap actionMap;

    private bool isEventBound;

    private void Awake()
    {
        actionMap = inputActions.actionMaps[0];
        actionMap.Enable();
    }

    private void OnEnable()
    {
        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    public void Initialize(NodeList nodeList)
    {
        playerControllInput.Initialize(nodeList);
    }

    private void BindEvents()
    {
        if (isEventBound) return;

        playerControllInput.ControllMenu += HandlerControllMenu;

        isEventBound = true;
    }

    private void UnbindEvents()
    {
        if (!isEventBound) return;

        playerControllInput.ControllMenu -= HandlerControllMenu;

        isEventBound = false;
    }

    private void HandlerControllMenu()
    {
        OnControllMenu?.Invoke();
    }
}
