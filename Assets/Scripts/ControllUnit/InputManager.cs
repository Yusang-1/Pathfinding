using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using Assets.Scripts.Controller;

namespace Assets.Scripts.ControllUnit
{
    public class InputManager : MonoBehaviour
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action OnControllMenu;

        public event Action OnSpawnUnitRequested;
        public event Action OnSetSpawnAreaFinished;
        public event Action<Vector3> OnTrackMouse;
        public event Action OnCancelSpawnAreaSet;
        public event Action<bool> OnPointerNotOverGameObject;

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInputComponent;
        [SerializeField] private SpawnAreaSetterInput spawnAreaSetterInput;

        [SerializeField] private InGamePlayerKeyboardMouseInput inGamePlayerKeyboardMouseInput;
        [SerializeField] private InGameUnitKeyboardMouseInput inGameUnitKeyboardMouseInput;
        [SerializeField] private InGamePlayerTouchInput inGamePlayerTouchInput;
        [SerializeField] private InGameUnitTouchInput inGameUnitTouchInput;

        [SerializeField] private PlayerController playerController;
        [SerializeField] private MoveScreenJudger moveScreenJudger;

        private InputActionMap actionMap;
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
            actionMap = inputActions.actionMaps[0];
            actionMap.Enable();

            // 현재 스키마 확인과 default로 사용할 actionMap지정
            var current = playerInputComponent.currentControlScheme;
            Debug.Log(current);

            if (current == KEYBOARD_MOUSE)
            {
                currentScheme = ControllScheme.KeyboardMouse;
                inGamePlayerKeyboardMouseInput.ActionMapActivated();
                defaultActionMap = ActionMaps.PlayerKeyboardMouse;
            }
            else if (current == TOUCH)
            {
                currentScheme = ControllScheme.Touch;
                inGamePlayerTouchInput.ActionMapActivated();
                defaultActionMap = ActionMaps.PlayerTouch;
            }
        }

        private void OnEnable()
        {
            BindEvents();
        }

        private void Start()
        {
            actionMapNameDict.Add(ActionMaps.PlayerKeyboardMouse, "Player");
            actionMapNameDict.Add(ActionMaps.PlayerTouch, "Player");
            actionMapNameDict.Add(ActionMaps.UnitKeyboardMouse, "Unit");
            actionMapNameDict.Add(ActionMaps.UnitTouch, "Unit");
            actionMapNameDict.Add(ActionMaps.SpawnAreaSetter, "SpawnAreaSetter");

            // 스키마별 actionMap 딕셔너리 생성
            var keyboardMouseInputerDict = new Dictionary<ActionMaps, IActionMapInputer>
            {
                { (inGamePlayerKeyboardMouseInput as IActionMapInputer).GetActionMap(), inGamePlayerKeyboardMouseInput },
                { (inGameUnitKeyboardMouseInput as IActionMapInputer).GetActionMap(), inGameUnitKeyboardMouseInput },
                { (spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput }
            };
            inputerSchemeDict.Add(ControllScheme.KeyboardMouse, keyboardMouseInputerDict);

            var touchInputerDict = new Dictionary<ActionMaps, IActionMapInputer>
            {
                { (inGamePlayerTouchInput as IActionMapInputer).GetActionMap(), inGamePlayerTouchInput },
                { (inGameUnitTouchInput as IActionMapInputer).GetActionMap(), inGameUnitTouchInput },
                { (spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput }
            };
            inputerSchemeDict.Add(ControllScheme.Touch, touchInputerDict);
        }

        private void Update()
        {
            if (currentScheme == ControllScheme.Touch)
            {
                touchInputMeditator.Update();
            }
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        public void Initialize(UnitSelector unitSelector)
        {
            unitSelector.GetActions(ChangeActionMapSelected, ChangeActionMapDefault);

            if (currentScheme == ControllScheme.KeyboardMouse)
            {
                inGamePlayerKeyboardMouseInput.Initialize(unitSelector, moveScreenJudger);
                inGameUnitKeyboardMouseInput.Initialize(unitSelector, moveScreenJudger);
                SetSchemeDict(currentScheme);
            }
            else if (currentScheme == ControllScheme.Touch)
            {
                inGamePlayerTouchInput.Initialize(unitSelector, moveScreenJudger);
                inGameUnitTouchInput.Initialize(unitSelector, moveScreenJudger);
                SetSchemeDict(currentScheme);
            }

            ChangeActionMapDefault();
        }

        private void SetSchemeDict(ControllScheme controllScheme)
        {
            currentInputerDict = inputerSchemeDict[controllScheme];
        }

        public void ChangeActionMapSelected(ActionMaps actionMap)
        {
            string actionMapName = actionMapNameDict[actionMap];

            playerInputComponent.SwitchCurrentActionMap(actionMapName);

            currentInputer?.ActionMapDeactivated();

            currentInputer = currentInputerDict[actionMap];

            currentInputer.ActionMapActivated();
        }

        private void ChangeActionMapDefault()
        {
            ChangeActionMapSelected(defaultActionMap);
        }

        private void BindEvents()
        {
            if (isEventBound) return;

            spawnAreaSetterInput.OnSetSpawnAreaFinished += ChangeActionMapDefault;
            spawnAreaSetterInput.OnSetSpawnAreaFinished += HandlerSetSpawnAreaFinished;
            spawnAreaSetterInput.OnSpawnUnitRequested += HandlerSpawnUnit;
            spawnAreaSetterInput.OnTrackMouse += HandlerTrackMouse;
            spawnAreaSetterInput.OnCancelSpawnAreaSet += HandlerCancelSpawnAreaSet;
            spawnAreaSetterInput.OnPointerNotOverGameObject += HandlePointerNotOverGameObject;

            if (inGamePlayerKeyboardMouseInput.IsActivated)
            {
                inGamePlayerKeyboardMouseInput.OnHoldStarted += HandlerHoldStarted;
                inGamePlayerKeyboardMouseInput.OnHoldPerformed += HandlerHoldPerformed;
                inGamePlayerKeyboardMouseInput.OnHoldCanceled += HandlerHoldCanceled;
                inGamePlayerKeyboardMouseInput.OnControllMenu += HandlerControllMenu;
                inGamePlayerKeyboardMouseInput.OnMoveScreen += playerController.SetVelocity;
                inGamePlayerKeyboardMouseInput.OnActionMapInputerActivated += keyboardMouseInputMeditator.AddBind;
                inGamePlayerKeyboardMouseInput.OnActionMapInputerDeactivated += keyboardMouseInputMeditator.RemoveBind;

                inGameUnitKeyboardMouseInput.OnHoldStarted += HandlerHoldStarted;
                inGameUnitKeyboardMouseInput.OnHoldPerformed += HandlerHoldPerformed;
                inGameUnitKeyboardMouseInput.OnHoldCanceled += HandlerHoldCanceled;
                inGameUnitKeyboardMouseInput.OnControllMenu += HandlerControllMenu;
                inGameUnitKeyboardMouseInput.OnMoveScreen += playerController.SetVelocity;
                inGameUnitKeyboardMouseInput.OnActionMapInputerActivated += keyboardMouseInputMeditator.AddBind;
                inGameUnitKeyboardMouseInput.OnActionMapInputerDeactivated += keyboardMouseInputMeditator.RemoveBind;

                keyboardMouseInputMeditator.OnZoomRequest += playerController.SetTargetZoom;
            }

            if (inGamePlayerTouchInput.IsActivated)
            {
                inGamePlayerTouchInput.OnHoldStarted += HandlerHoldStarted;
                inGamePlayerTouchInput.OnHoldPerformed += HandlerHoldPerformed;
                inGamePlayerTouchInput.OnHoldCanceled += HandlerHoldCanceled;
                inGamePlayerTouchInput.OnMoveScreen += playerController.SetVelocity;
                inGamePlayerTouchInput.OnActionMapInputerActivated += touchInputMeditator.SetTouchInput;

                inGameUnitTouchInput.OnHoldStarted += HandlerHoldStarted;
                inGameUnitTouchInput.OnHoldPerformed += HandlerHoldPerformed;
                inGameUnitTouchInput.OnHoldCanceled += HandlerHoldCanceled;
                inGameUnitTouchInput.OnMoveScreen += playerController.SetVelocity;
                inGameUnitTouchInput.OnActionMapInputerActivated += touchInputMeditator.SetTouchInput;
                
                touchInputMeditator.OnZoomRequest += playerController.SetTargetZoom;
            }

            isEventBound = true;
        }

        private void UnbindEvents()
        {
            if (!isEventBound) return;

            spawnAreaSetterInput.OnSetSpawnAreaFinished -= ChangeActionMapDefault;
            spawnAreaSetterInput.OnSetSpawnAreaFinished -= HandlerSetSpawnAreaFinished;
            spawnAreaSetterInput.OnSpawnUnitRequested -= HandlerSpawnUnit;
            spawnAreaSetterInput.OnTrackMouse -= HandlerTrackMouse;
            spawnAreaSetterInput.OnCancelSpawnAreaSet -= HandlerCancelSpawnAreaSet;
            spawnAreaSetterInput.OnPointerNotOverGameObject -= HandlePointerNotOverGameObject;

            if (inGamePlayerKeyboardMouseInput.IsActivated)
            {
                inGamePlayerKeyboardMouseInput.OnHoldStarted -= HandlerHoldStarted;
                inGamePlayerKeyboardMouseInput.OnHoldPerformed -= HandlerHoldPerformed;
                inGamePlayerKeyboardMouseInput.OnHoldCanceled -= HandlerHoldCanceled;
                inGamePlayerKeyboardMouseInput.OnControllMenu -= HandlerControllMenu;
                inGamePlayerKeyboardMouseInput.OnMoveScreen -= playerController.SetVelocity;
                inGamePlayerKeyboardMouseInput.OnActionMapInputerActivated -= keyboardMouseInputMeditator.AddBind;
                inGamePlayerKeyboardMouseInput.OnActionMapInputerDeactivated -= keyboardMouseInputMeditator.RemoveBind;

                inGameUnitKeyboardMouseInput.OnHoldStarted -= HandlerHoldStarted;
                inGameUnitKeyboardMouseInput.OnHoldPerformed -= HandlerHoldPerformed;
                inGameUnitKeyboardMouseInput.OnHoldCanceled -= HandlerHoldCanceled;
                inGameUnitKeyboardMouseInput.OnControllMenu -= HandlerControllMenu;
                inGameUnitKeyboardMouseInput.OnMoveScreen -= playerController.SetVelocity;
                inGameUnitKeyboardMouseInput.OnActionMapInputerActivated -= keyboardMouseInputMeditator.AddBind;
                inGameUnitKeyboardMouseInput.OnActionMapInputerDeactivated -= keyboardMouseInputMeditator.RemoveBind;

                keyboardMouseInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;
            }

            if (inGamePlayerTouchInput.IsActivated)
            {
                inGamePlayerTouchInput.OnHoldStarted -= HandlerHoldStarted;
                inGamePlayerTouchInput.OnHoldPerformed -= HandlerHoldPerformed;
                inGamePlayerTouchInput.OnHoldCanceled -= HandlerHoldCanceled;
                inGamePlayerTouchInput.OnMoveScreen -= playerController.SetVelocity;
                inGamePlayerTouchInput.OnActionMapInputerActivated -= touchInputMeditator.SetTouchInput;

                inGameUnitTouchInput.OnHoldStarted -= HandlerHoldStarted;
                inGameUnitTouchInput.OnHoldPerformed -= HandlerHoldPerformed;
                inGameUnitTouchInput.OnHoldCanceled -= HandlerHoldCanceled;
                inGameUnitTouchInput.OnMoveScreen -= playerController.SetVelocity;
                inGameUnitTouchInput.OnActionMapInputerActivated -= touchInputMeditator.SetTouchInput;
                
                touchInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;
            }

            isEventBound = false;
        }

        private void HandlerHoldStarted(Vector3 vec)
        {
            OnHoldStarted?.Invoke(vec);
        }
        private void HandlerHoldPerformed(Vector3 vec)
        {
            OnHoldPerformed?.Invoke(vec);
        }
        private void HandlerHoldCanceled()
        {
            OnHoldCanceled?.Invoke();
        }
        private void HandlerControllMenu()
        {
            OnControllMenu?.Invoke();
        }
        private void HandlerSpawnUnit()
        {
            OnSpawnUnitRequested?.Invoke();
        }
        private void HandlerSetSpawnAreaFinished()
        {
            OnSetSpawnAreaFinished?.Invoke();
        }
        private void HandlerTrackMouse(Vector3 pos)
        {
            OnTrackMouse?.Invoke(pos);
        }
        private void HandlerCancelSpawnAreaSet()
        {
            OnCancelSpawnAreaSet?.Invoke();
        }
        private void HandlePointerNotOverGameObject(bool value)
        {
            OnPointerNotOverGameObject?.Invoke(value);
        }
    }
}

public interface IActionMapInputer
{
    public ActionMaps GetActionMap();
    public void ActionMapActivated();
    public void ActionMapDeactivated();
}

