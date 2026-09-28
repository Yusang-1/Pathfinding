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

        [SerializeField] private InGamePlayerKeyboardMouseInputECS inGamePlayerKeyboardMouseInput;
        [SerializeField] private InGameUnitKeyboardMouseInputECS inGameUnitKeyboardMouseInput;
        [SerializeField] private InGamePlayerTouchInputECS inGamePlayerTouchInput;
        [SerializeField] private InGameUnitTouchInputECS inGameUnitTouchInput;

        [SerializeField] private PlayerController playerController;
        [SerializeField] private MoveScreenJudger moveScreenJudger;

        private InputActionMap actionMap;
        private IActionMapInputer currentInputer;
        private readonly KeyboardMouseInputPlayerControllerMeditator keyboardMouseInputMeditator = new();
        private readonly TouchInputPlayerControllerMeditator touchInputMeditator = new();

        private readonly Dictionary<ActionMaps, string> actionMapNameDict = new();
        private readonly Dictionary<ControllScheme, Dictionary<ActionMaps, IActionMapInputer>> inputerSchemeDict = new();
        private Dictionary<ActionMaps, IActionMapInputer> currentInputerDict = new();

        private EntityQuery changeActionMapQuery;

        private bool isEventBound;
        private ControllScheme currentScheme;
        private ActionMaps defaultActionMap;
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
                inGamePlayerKeyboardMouseInput.ActionMapActivated();
                defaultActionMap = ActionMaps.PlayerKeyboardMouse;
            }
            else if (current == TOUCH)
            {
                currentScheme = ControllScheme.Touch;
                inGamePlayerTouchInput.ActionMapActivated();
                defaultActionMap = ActionMaps.PlayerTouch;
            }
            
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

            var touchInputerDict = new Dictionary<ActionMaps, IActionMapInputer>
            {
                { (inGamePlayerTouchInput as IActionMapInputer).GetActionMap(), inGamePlayerTouchInput },
                { (inGameUnitTouchInput as IActionMapInputer).GetActionMap(), inGameUnitTouchInput },
                { (spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput }
            };
            inputerSchemeDict.Add(ControllScheme.Touch, touchInputerDict);
        }

        private void OnEnable()
        {
            BindEvents();
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

            if (currentScheme == ControllScheme.KeyboardMouse)
            {
                inGamePlayerKeyboardMouseInput.Initialize(selectableController, moveScreenJudger);
                inGameUnitKeyboardMouseInput.Initialize(selectableController, moveScreenJudger);
                SetSchemeDict(currentScheme);
            }
            else if (currentScheme == ControllScheme.Touch)
            {
                inGamePlayerTouchInput.Initialize(selectableController, moveScreenJudger);
                inGameUnitTouchInput.Initialize(selectableController, moveScreenJudger);
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

                inGameUnitTouchInput.OnHoldStarted += HandlerHoldStarted;
                inGameUnitTouchInput.OnHoldPerformed += HandlerHoldPerformed;
                inGameUnitTouchInput.OnHoldCanceled += HandlerHoldCanceled;
                inGameUnitTouchInput.OnMoveScreen += playerController.SetVelocity;
                                
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

                inGameUnitTouchInput.OnHoldStarted -= HandlerHoldStarted;
                inGameUnitTouchInput.OnHoldPerformed -= HandlerHoldPerformed;
                inGameUnitTouchInput.OnHoldCanceled -= HandlerHoldCanceled;
                inGameUnitTouchInput.OnMoveScreen -= playerController.SetVelocity;
                
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