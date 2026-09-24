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
        [SerializeField] private PlayerControllInput playerControllerInput;
        [SerializeField] private UnitInput unitInput;
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
            actionMapNameDict.Add(ActionMaps.Player, "Player");
            actionMapNameDict.Add(ActionMaps.Unit, "Unit");
            actionMapNameDict.Add(ActionMaps.SpawnAreaSetter, "SpawnAreaSetter");

            inputerDict.Add((playerControllerInput as IActionMapInputer).GetActionMap(), playerControllerInput);
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
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        public void Initialize(UnitSelector unitSelector)
        {
            unitSelector.GetActions(ChangeActionMapSelected, ChangeActionMapDefault);

            playerControllerInput.Initialize(unitSelector);
            unitInput.Initialize(unitSelector);
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

            playerControllerInput.OnHoldStarted += HandlerHoldStarted;
            playerControllerInput.OnHoldPerformed += HandlerHoldPerformed;
            playerControllerInput.OnHoldCanceled += HandlerHoldCanceled;
            playerControllerInput.OnControllMenu += HandlerControllMenu;

            unitInput.OnHoldStarted += HandlerHoldStarted;
            unitInput.OnHoldPerformed += HandlerHoldPerformed;
            unitInput.OnHoldCanceled += HandlerHoldCanceled;
            unitInput.OnControllMenu += HandlerControllMenu;

            spawnAreaSetterInput.OnSetSpawnAreaFinished += ChangeActionMapDefault;
            spawnAreaSetterInput.OnSetSpawnAreaFinished += HandlerSetSpawnAreaFinished;
            spawnAreaSetterInput.OnSpawnUnitRequested += HandlerSpawnUnit;
            spawnAreaSetterInput.OnTrackMouse += HandlerTrackMouse;
            spawnAreaSetterInput.OnCancelSpawnAreaSet += HandlerCancelSpawnAreaSet;
            spawnAreaSetterInput.OnPointerNotOverGameObject += HandlePointerNotOverGameObject;

            keyboardMouseInputMeditator.OnDirectionChanged += playerController.SetDirection;
            keyboardMouseInputMeditator.OnZoomRequest += playerController.SetTargetZoom;

            touchInputMeditator.OnDirectionChanged += playerController.SetDirection;
            touchInputMeditator.OnZoomRequest += playerController.SetTargetZoom;

            isEventBound = true;
        }

        private void UnbindEvents()
        {
            if (!isEventBound) return;

            playerControllerInput.OnHoldStarted -= HandlerHoldStarted;
            playerControllerInput.OnHoldPerformed -= HandlerHoldPerformed;
            playerControllerInput.OnHoldCanceled -= HandlerHoldCanceled;
            playerControllerInput.OnControllMenu -= HandlerControllMenu;

            unitInput.OnHoldStarted -= HandlerHoldStarted;
            unitInput.OnHoldPerformed -= HandlerHoldPerformed;
            unitInput.OnHoldCanceled -= HandlerHoldCanceled;
            unitInput.OnControllMenu -= HandlerControllMenu;

            spawnAreaSetterInput.OnSetSpawnAreaFinished -= ChangeActionMapDefault;
            spawnAreaSetterInput.OnSetSpawnAreaFinished -= HandlerSetSpawnAreaFinished;
            spawnAreaSetterInput.OnSpawnUnitRequested -= HandlerSpawnUnit;
            spawnAreaSetterInput.OnTrackMouse -= HandlerTrackMouse;
            spawnAreaSetterInput.OnCancelSpawnAreaSet -= HandlerCancelSpawnAreaSet;
            spawnAreaSetterInput.OnPointerNotOverGameObject -= HandlePointerNotOverGameObject;
            
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

