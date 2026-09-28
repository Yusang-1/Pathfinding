using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Assets.Scripts.Controller;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public event Action OnControllMenu;

    [SerializeField] private PlayerInput playerInputComponent;

    [SerializeField] private CreateMapKeyboardMouseInput createMapKeyboardMouseInput;
    [SerializeField] private CreateMapTouchInput createMapTouchInput;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private MoveScreenJudger moveScreenJudger;

    private readonly SelectableController selectableController = new();
    private IActionMapInputer currentInputer;
    private readonly KeyboardMouseInputPlayerControllerMeditator keyboardMouseInputMeditator = new();
    private readonly TouchInputPlayerControllerMeditator touchInputMeditator = new();

    private readonly Dictionary<ActionMaps, string> actionMapNameDict = new();
    private readonly Dictionary<ControllScheme, Dictionary<ActionMaps, IActionMapInputer>> inputerSchemeDict = new();
    private Dictionary<ActionMaps, IActionMapInputer> currentInputerDict = new();

    private ControllScheme currentScheme;
    private ActionMaps defaultActionMap;
    private bool isEventBound;
    private const string KEYBOARD_MOUSE = "Keyboard&Mouse";
    private const string TOUCH = "Touch";

    private enum ControllScheme
    {
        KeyboardMouse,
        Touch
    }

    private void Awake()
    {
        var current = playerInputComponent.currentControlScheme;
        Debug.Log(current);
        if (current == KEYBOARD_MOUSE)
        {
            currentScheme = ControllScheme.KeyboardMouse;
            defaultActionMap = ActionMaps.DefaultKeyboardMouse;
        }
        else if (current == TOUCH)
        {
            currentScheme = ControllScheme.Touch;
            defaultActionMap = ActionMaps.DefaultTouch;
        }

        actionMapNameDict.Add(ActionMaps.DefaultKeyboardMouse, "Player");
        actionMapNameDict.Add(ActionMaps.DefaultTouch, "Player");
        actionMapNameDict.Add(ActionMaps.SpawnAreaSetter, "SpawnAreaSetter");

        // 스키마별 actionMap 딕셔너리 생성
        var keyboardMouseInputerDict = new Dictionary<ActionMaps, IActionMapInputer>
            {
                { (createMapKeyboardMouseInput as IActionMapInputer).GetActionMap(), createMapKeyboardMouseInput }
            };
        inputerSchemeDict.Add(ControllScheme.KeyboardMouse, keyboardMouseInputerDict);

        var touchInputerDict = new Dictionary<ActionMaps, IActionMapInputer>
            {
                { (createMapTouchInput as IActionMapInputer).GetActionMap(), createMapTouchInput }
            };
        inputerSchemeDict.Add(ControllScheme.Touch, touchInputerDict);
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
        if (currentScheme == ControllScheme.KeyboardMouse)
        {
            createMapKeyboardMouseInput.Initialize(selectableController, moveScreenJudger, nodeList);
            SetSchemeDict(currentScheme);
        }
        else if (currentScheme == ControllScheme.Touch)
        {
            createMapTouchInput.Initialize(selectableController, moveScreenJudger, nodeList);
            SetSchemeDict(currentScheme);
        }

        ChangeActionMapDefault();
    }

    private void SetSchemeDict(ControllScheme controllScheme)
    {
        currentInputerDict = inputerSchemeDict[controllScheme];
    }

    private void ChangeActionMapDefault()
    {
        ChangeActionMapSelected(defaultActionMap);
    }

    public void ChangeActionMapSelected(ActionMaps actionMap)
    {
        string actionMapName = actionMapNameDict[actionMap];

        playerInputComponent.SwitchCurrentActionMap(actionMapName);

        currentInputer?.ActionMapDeactivated();

        currentInputer = currentInputerDict[actionMap];

        currentInputer.ActionMapActivated();
    }

    private void BindEvents()
    {
        if (isEventBound) return;

        var current = playerInputComponent.currentControlScheme;
        if (current == KEYBOARD_MOUSE)
        {
            createMapKeyboardMouseInput.OnControllMenu += HandlerControllMenu;
            createMapKeyboardMouseInput.OnMoveScreen += playerController.SetVelocity;
            createMapKeyboardMouseInput.OnActionMapInputerActivated += keyboardMouseInputMeditator.AddBind;
            createMapKeyboardMouseInput.OnActionMapInputerDeactivated += keyboardMouseInputMeditator.RemoveBind;

            keyboardMouseInputMeditator.OnZoomRequest += playerController.SetTargetZoom;
        }

        if (current == TOUCH)
        {
            // createMapTouchInput.OnControllMenu += HandlerControllMenu;
            createMapTouchInput.OnMoveScreen += playerController.SetVelocity;
            createMapTouchInput.OnActionMapInputerActivated += touchInputMeditator.SetTouchInput;

            // touchInputMeditator.OnDirectionChanged += playerController.SetDirection;
            touchInputMeditator.OnZoomRequest += playerController.SetTargetZoom;
        }

        isEventBound = true;
    }

    private void UnbindEvents()
    {
        if (!isEventBound) return;

        var current = playerInputComponent.currentControlScheme;
        if (current == KEYBOARD_MOUSE)
        {
            createMapKeyboardMouseInput.OnControllMenu -= HandlerControllMenu;
            createMapKeyboardMouseInput.OnMoveScreen -= playerController.SetVelocity;
            createMapKeyboardMouseInput.OnActionMapInputerActivated -= keyboardMouseInputMeditator.AddBind;
            createMapKeyboardMouseInput.OnActionMapInputerDeactivated -= keyboardMouseInputMeditator.RemoveBind;

            keyboardMouseInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;
        }

        if (current == TOUCH)
        {
            // createMapTouchInput.OnControllMenu -= HandlerControllMenu;
            createMapTouchInput.OnMoveScreen -= playerController.SetVelocity;
            createMapTouchInput.OnActionMapInputerActivated -= touchInputMeditator.SetTouchInput;

            // touchInputMeditator.OnDirectionChanged -= playerController.SetDirection;
            touchInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;
        }

        isEventBound = false;
    }

    private void HandlerControllMenu()
    {
        OnControllMenu?.Invoke();
    }
}
