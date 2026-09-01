using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public event Action OnControllMenu;

    [SerializeField] private InputActionAsset inputActions;
    private PlayerControllInput playerControllInput;
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

    private void Start()
    {
        playerControllInput ??= GetComponent<PlayerControllInput>();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void BindEvents()
    {
        if(isEventBound) return;
        
        playerControllInput ??= GetComponent<PlayerControllInput>();
        playerControllInput.ControllMenu += HandlerControllMenu;
        
        isEventBound = true;
    }

    private void UnbindEvents()
    {
        if(!isEventBound) return;
        
        playerControllInput.ControllMenu -= HandlerControllMenu;
        
        isEventBound = false;
    }

    private void HandlerControllMenu()
    {
        OnControllMenu?.Invoke();
    }
}
