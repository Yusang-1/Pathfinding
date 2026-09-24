using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Assets.Scripts.Controller;

public class InputManager : MonoBehaviour
{
    public event Action OnControllMenu;

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private PlayerInput playerInputComponent;
    [SerializeField] private PlayerControllInput playerControllInput;
    [SerializeField] private TouchInput touchInput;
    [SerializeField] private KeyboardMouseInput keyboardMouseInput;

    [SerializeField] private PlayerController playerController;
    private KeyboardMouseInputPlayerControllerMeditator keyboardMouseInputMeditator;
    private TouchInputPlayerControllerMeditator touchInputMeditator;

    private InputActionMap actionMap;

    private bool isEventBound;
    private ControllScheme currentScheme;
    private const string KEYBOARD_MOUSE = "Keyboard&Mouse";
    private const string TOUCH = "Touch";

    private enum ControllScheme
    {
        KeyboardMouse,
        Touch
    }

    private void Awake()
    {
        actionMap = inputActions.actionMaps[0];
        actionMap.Enable();

        var current = playerInputComponent.currentControlScheme;
        if (current == KEYBOARD_MOUSE)
        {
            currentScheme = ControllScheme.KeyboardMouse;
        }
        else if (current == TOUCH)
        {
            currentScheme = ControllScheme.Touch;
        }

        keyboardMouseInputMeditator = new KeyboardMouseInputPlayerControllerMeditator(keyboardMouseInput);
        touchInputMeditator = new TouchInputPlayerControllerMeditator(touchInput);
    }

    private void OnEnable()
    {
        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void Update()
    {
        if (currentScheme == ControllScheme.Touch)
        {
            touchInputMeditator.Update();
        }
    }

    public void Initialize(NodeList nodeList)
    {
        playerControllInput.Initialize(nodeList);
    }

    private void BindEvents()
    {
        if (isEventBound) return;

        playerControllInput.ControllMenu += HandlerControllMenu;

        keyboardMouseInputMeditator.OnDirectionChanged += playerController.SetDirection;
        keyboardMouseInputMeditator.OnZoomRequest += playerController.SetTargetZoom;

        touchInputMeditator.OnDirectionChanged += playerController.SetDirection;
        touchInputMeditator.OnZoomRequest += playerController.SetTargetZoom;

        isEventBound = true;
    }

    private void UnbindEvents()
    {
        if (!isEventBound) return;

        playerControllInput.ControllMenu -= HandlerControllMenu;

        keyboardMouseInputMeditator.OnDirectionChanged -= playerController.SetDirection;
        keyboardMouseInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;

        touchInputMeditator.OnDirectionChanged -= playerController.SetDirection;
        touchInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;

        isEventBound = false;
    }

    private void HandlerControllMenu()
    {
        OnControllMenu?.Invoke();
    }
}
