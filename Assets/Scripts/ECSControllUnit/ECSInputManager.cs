using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Collections;
using Unity.Entities;
using System;
using System.Collections.Generic;
using Assets.Scripts.Controller;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSInputManager : MonoBehaviour
    {
        public event Action<Vector3> OnHoldStarted;
        public event Func<Vector3, Vector3?> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action OnControllMenu;

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInputComponent;
        [SerializeField] private SpawnAreaSetterInput spawnAreaSetterInput;
        [SerializeField] private TouchInput touchInput;

        [SerializeField] private InGamePlayerKeyboardMouseInput inGamePlayerKeyboardMouseInput;
        [SerializeField] private InGameUnitKeyboardMouseInput inGameUnitKeyboardMouseInput;

        [SerializeField] private PlayerController playerController;
        [SerializeField] private MoveScreenJudger moveScreenJudger;

        private InputActionMap actionMap;
        private IActionMapInputer currentInputer;
        private readonly KeyboardMouseInputPlayerControllerMeditator keyboardMouseInputMeditator;
        private TouchInputPlayerControllerMeditator touchInputMeditator;

        private readonly Dictionary<ActionMaps, string> actionMapNameDict = new();
        private readonly Dictionary<ControllScheme, Dictionary<ActionMaps, IActionMapInputer>> inputerSchemeDict = new();
        private Dictionary<ActionMaps, IActionMapInputer> currentInputerDict = new();

        private EntityQuery changeActionMapQuery;

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
        }

        private void OnEnable()
        {
            BindEvents();
        }

        private void Start()
        {
            changeActionMapQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(ChangeActionMapRequest));

            actionMapNameDict.Add(ActionMaps.PlayerKeyboardMouse, "Player");
            actionMapNameDict.Add(ActionMaps.PlayerTouch, "Player");
            actionMapNameDict.Add(ActionMaps.UnitKeyboardMouse, "Unit");
            actionMapNameDict.Add(ActionMaps.UnitTouch, "Unit");
            actionMapNameDict.Add(ActionMaps.SpawnAreaSetter, "SpawnAreaSetter");

            var keyboardMouseInputerDict = new Dictionary<ActionMaps, IActionMapInputer>
            {
                { (inGamePlayerKeyboardMouseInput as IActionMapInputer).GetActionMap(), inGamePlayerKeyboardMouseInput },
                { (inGameUnitKeyboardMouseInput as IActionMapInputer).GetActionMap(), inGameUnitKeyboardMouseInput },
                { (spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput }
            };
            inputerSchemeDict.Add(ControllScheme.KeyboardMouse, keyboardMouseInputerDict);

            var touchInputerDict = new Dictionary<ActionMaps, IActionMapInputer>();
            // 
            inputerSchemeDict.Add(ControllScheme.Touch, touchInputerDict);

            ChangeActionMapDefault();
        }

        private void Update()
        {
            if (currentScheme == ControllScheme.Touch)
            {
                touchInputMeditator.Update();
            }

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            using var requests = changeActionMapQuery.ToComponentDataArray<ChangeActionMapRequest>(Allocator.Temp);
            if (requests == null || requests.Length == 0)
            {
                return;
            }

            using var entities = changeActionMapQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < requests.Length; i++)
            {
                ChangeActionMapSelected(requests[i].TargetMap);
                entityManager.DestroyEntity(entities[i]);
            }
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        public void Initialize(ECSSelectableController selectableController)
        {
            selectableController.GetActions(ChangeActionMapSelected, ChangeActionMapDefault);

            // playerControllInput.Initialize(selectableController);
            // unitInput.Initialize(selectableController);

            if (currentScheme == ControllScheme.KeyboardMouse)
            {
                // inGamePlayerKeyboardMouseInput.Initialize(unitSelector, moveScreenJudger);
                // inGameUnitKeyboardMouseInput.Initialize(unitSelector, moveScreenJudger);
                SetSchemeDict(currentScheme);
            }
            else if (currentScheme == ControllScheme.Touch)
            {
                // touchInput.Initialize(unitSelector);
                SetSchemeDict(currentScheme);
            }
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

        private const ActionMaps DefaultActionMap = ActionMaps.PlayerKeyboardMouse;
        private void ChangeActionMapDefault()
        {
            ChangeActionMapSelected(DefaultActionMap);
        }

        private void BindEvents()
        {
            if (isEventBound) return;

            if (inGamePlayerKeyboardMouseInput.IsActivated)
            {
                inGamePlayerKeyboardMouseInput.OnHoldStarted += HandlerHoldStarted;
                // inGamePlayerKeyboardMouseInput.OnHoldPerformed += HandlerHoldPerformed;
                inGamePlayerKeyboardMouseInput.OnHoldCanceled += HandlerHoldCanceled;
                inGamePlayerKeyboardMouseInput.OnControllMenu += HandlerControllMenu;
                inGamePlayerKeyboardMouseInput.OnActionMapInputerActivated += keyboardMouseInputMeditator.AddBind;
                inGamePlayerKeyboardMouseInput.OnActionMapInputerDeactivated += keyboardMouseInputMeditator.RemoveBind;

                inGameUnitKeyboardMouseInput.OnHoldStarted += HandlerHoldStarted;
                // inGameUnitKeyboardMouseInput.OnHoldPerformed += HandlerHoldPerformed;
                inGameUnitKeyboardMouseInput.OnHoldCanceled += HandlerHoldCanceled;
                inGameUnitKeyboardMouseInput.OnControllMenu += HandlerControllMenu;
                inGameUnitKeyboardMouseInput.OnActionMapInputerActivated += keyboardMouseInputMeditator.AddBind;
                inGameUnitKeyboardMouseInput.OnActionMapInputerDeactivated += keyboardMouseInputMeditator.RemoveBind;

                keyboardMouseInputMeditator.OnDirectionChanged += playerController.SetDirection;
                keyboardMouseInputMeditator.OnZoomRequest += playerController.SetTargetZoom;
            }

            if (touchInput.IsActivated())
            {
                touchInput.OnHoldStarted += HandlerHoldStarted;
                // touchInput.OnHoldPerformed += HandlerHoldPerformed;
                touchInput.OnHoldCanceled += HandlerHoldCanceled;

                touchInputMeditator.OnDirectionChanged += playerController.SetDirection;
                touchInputMeditator.OnZoomRequest += playerController.SetTargetZoom;
            }

            spawnAreaSetterInput.OnSetSpawnAreaFinished += ChangeActionMapDefault;

            isEventBound = true;
        }

        private void UnbindEvents()
        {
            if (!isEventBound) return;

            if (inGamePlayerKeyboardMouseInput.IsActivated)
            {
                inGamePlayerKeyboardMouseInput.OnHoldStarted -= HandlerHoldStarted;
                // inGamePlayerKeyboardMouseInput.OnHoldPerformed -= HandlerHoldPerformed;
                inGamePlayerKeyboardMouseInput.OnHoldCanceled -= HandlerHoldCanceled;
                inGamePlayerKeyboardMouseInput.OnControllMenu -= HandlerControllMenu;
                inGamePlayerKeyboardMouseInput.OnActionMapInputerActivated -= keyboardMouseInputMeditator.AddBind;
                inGamePlayerKeyboardMouseInput.OnActionMapInputerDeactivated -= keyboardMouseInputMeditator.RemoveBind;

                inGameUnitKeyboardMouseInput.OnHoldStarted -= HandlerHoldStarted;
                // inGameUnitKeyboardMouseInput.OnHoldPerformed -= HandlerHoldPerformed;
                inGameUnitKeyboardMouseInput.OnHoldCanceled -= HandlerHoldCanceled;
                inGameUnitKeyboardMouseInput.OnControllMenu -= HandlerControllMenu;
                inGameUnitKeyboardMouseInput.OnActionMapInputerActivated -= keyboardMouseInputMeditator.AddBind;
                inGameUnitKeyboardMouseInput.OnActionMapInputerDeactivated -= keyboardMouseInputMeditator.RemoveBind;

                keyboardMouseInputMeditator.OnDirectionChanged -= playerController.SetDirection;
                keyboardMouseInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;
            }

            if (touchInput.IsActivated())
            {
                touchInput.OnHoldStarted -= HandlerHoldStarted;
                // touchInput.OnHoldPerformed -= HandlerHoldPerformed;
                touchInput.OnHoldCanceled -= HandlerHoldCanceled;

                touchInputMeditator.OnDirectionChanged -= playerController.SetDirection;
                touchInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;
            }

            spawnAreaSetterInput.OnSetSpawnAreaFinished -= ChangeActionMapDefault;

            isEventBound = false;
        }

        private void HandlerHoldStarted(Vector3 vec)
        {
            OnHoldStarted?.Invoke(vec);
        }
        private Vector3? HandlerHoldPerformed(Vector3 vec)
        {
            return OnHoldPerformed?.Invoke(vec);
        }
        private void HandlerHoldCanceled()
        {
            OnHoldCanceled?.Invoke();
        }
        private void HandlerControllMenu()
        {
            OnControllMenu?.Invoke();
        }
    }
}

public enum ActionMaps
{
    PlayerKeyboardMouse,
    PlayerTouch,
    UnitKeyboardMouse,
    UnitTouch,
    SpawnAreaSetter,
    DefaultKeyboardMouse,
    DefaultTouch,
    None
}