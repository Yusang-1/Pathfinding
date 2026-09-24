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
        public event Func<Vector3,Vector3?> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action OnControllMenu;        

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInputComponent;
        [SerializeField] private ECSPlayerControllInput playerControllInput;
        [SerializeField] private ECSUnitInput unitInput;
        [SerializeField] private SpawnAreaSetterInput spawnAreaSetterInput;
        [SerializeField] private TouchInput touchInput;
        [SerializeField] private KeyboardMouseInput keyboardMouseInput;

        [SerializeField] private PlayerController playerController;

        private InputActionMap actionMap;
        private IActionMapInputer currentInputer;
        private KeyboardMouseInputPlayerControllerMeditator keyboardMouseInputMeditator;
        private TouchInputPlayerControllerMeditator touchInputMeditator;
        
        private readonly Dictionary<ActionMaps, string> actionMapNameDict = new();
        private readonly Dictionary<ActionMaps, IActionMapInputer> inputerDict = new();

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
            if(current == KEYBOARD_MOUSE)
            {
                currentScheme = ControllScheme.KeyboardMouse;
            }
            else if(current == TOUCH)
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

        private void Start()
        {
            changeActionMapQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(ChangeActionMapRequest));

            actionMapNameDict.Add(ActionMaps.Player, "Player");
            actionMapNameDict.Add(ActionMaps.Unit, "Unit");
            actionMapNameDict.Add(ActionMaps.SpawnAreaSetter, "SpawnAreaSetter");

            inputerDict.Add((playerControllInput as IActionMapInputer).GetActionMap(), playerControllInput);
            inputerDict.Add((unitInput as IActionMapInputer).GetActionMap(), unitInput);
            inputerDict.Add((spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput);

            ChangeActionMapDefault();
        }

        private void Update()
        {
            if(currentScheme == ControllScheme.Touch)
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

            playerControllInput.Initialize(selectableController);
            unitInput.Initialize(selectableController);
        }

        public void ChangeActionMapSelected(ActionMaps actionMap)
        {
            string actionMapName = actionMapNameDict[actionMap];

            playerInputComponent.SwitchCurrentActionMap(actionMapName);

            currentInputer?.ActionMapDeactivated();

            currentInputer = inputerDict[actionMap];

            currentInputer.ActionMapActivated();
        }

        private const ActionMaps DefaultActionMap = ActionMaps.Player;
        private void ChangeActionMapDefault()
        {
            ChangeActionMapSelected(DefaultActionMap);
        }

        private void BindEvents()
        {
            if (isEventBound) return;

            playerControllInput.OnHoldStarted += HandlerHoldStarted;
            playerControllInput.OnHoldPerformed += HandlerHoldPerformed;
            playerControllInput.OnHoldCanceled += HandlerHoldCanceled;
            playerControllInput.OnControllMenu += HandlerControllMenu;

            unitInput.OnHoldStarted += HandlerHoldStarted;
            unitInput.OnHoldPerformed += HandlerHoldPerformed;
            unitInput.OnHoldCanceled += HandlerHoldCanceled;
            unitInput.OnControllMenu += HandlerControllMenu;
            
            spawnAreaSetterInput.OnSetSpawnAreaFinished += ChangeActionMapDefault;
            
            keyboardMouseInputMeditator.OnDirectionChanged += playerController.SetDirection;
            keyboardMouseInputMeditator.OnZoomRequest += playerController.SetTargetZoom;

            touchInputMeditator.OnDirectionChanged += playerController.SetDirection;
            touchInputMeditator.OnZoomRequest += playerController.SetTargetZoom;

            isEventBound = true;
        }

        private void UnbindEvents()
        {
            if (!isEventBound) return;

            playerControllInput.OnHoldStarted -= HandlerHoldStarted;
            playerControllInput.OnHoldPerformed -= HandlerHoldPerformed;
            playerControllInput.OnHoldCanceled -= HandlerHoldCanceled;
            playerControllInput.OnControllMenu -= HandlerControllMenu;

            unitInput.OnHoldStarted -= HandlerHoldStarted;
            unitInput.OnHoldPerformed -= HandlerHoldPerformed;
            unitInput.OnHoldCanceled -= HandlerHoldCanceled;
            unitInput.OnControllMenu -= HandlerControllMenu;
            
            spawnAreaSetterInput.OnSetSpawnAreaFinished -= ChangeActionMapDefault;
            
            keyboardMouseInputMeditator.OnDirectionChanged -= playerController.SetDirection;
            keyboardMouseInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;

            touchInputMeditator.OnDirectionChanged -= playerController.SetDirection;
            touchInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;

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
    Player,
    Unit,
    SpawnAreaSetter
}