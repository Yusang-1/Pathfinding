using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public event Action ControllMenu;
    
    [SerializeField] private InputActionAsset inputActions;
    private PlayerControllInput playerControllInput;

    private InputActionMap actionMap;

    private void Awake()
    {
        actionMap = inputActions.actionMaps[0];
        actionMap.Enable();
    }
    
    private void Start()
    {
        playerControllInput = GetComponent<PlayerControllInput>();
        playerControllInput.ControllMenu += () => ControllMenu?.Invoke();
    }
}
