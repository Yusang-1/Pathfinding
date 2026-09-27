using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using Assets.Scripts.Controller;

namespace Assets.Scripts.CreateMap
{
    public class InputManager : MonoBehaviour
    {
        public event Action OnControllMenu;
        public event Action OnSpawnUnitRequested;
        public event Action OnSetSpawnAreaFinished;
        public event Action<Vector3> OnTrackMouse;
        public event Action OnCancelSpawnAreaSet;
        public event Action<bool> OnPointerNotOverGameObject;

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInputComponent;
        [SerializeField] private PlayerControllInput playerControllerInput;
        [SerializeField] private SpawnAreaSetterInput spawnAreaSetterInput;
        
        [SerializeField] private InGamePlayerKeyboardMouseInput inGamePlayerKeyboardMouseInput;
        [SerializeField] private InGameUnitKeyboardMouseInput inGameUnitKeyboardMouseInput;
        
        [SerializeField] private PlayerController playerController;
        [SerializeField] private MoveScreenJudger moveScreenJudger;

        private SelectableController selectableController;
        private IActionMapInputer currentInputer;
        private KeyboardMouseInputPlayerControllerMeditator keyboardMouseInputMeditator;
        private TouchInputPlayerControllerMeditator touchInputMeditator;

        private readonly Dictionary<ActionMaps, string> actionMapNameDict = new();
        private readonly Dictionary<ActionMaps, IActionMapInputer> inputerDict = new();

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
            if (current == KEYBOARD_MOUSE)
            {
                currentScheme = ControllScheme.KeyboardMouse;
                defaultActionMap = ActionMaps.PlayerKeyboardMouse;
            }
            else if (current == TOUCH)
            {
                currentScheme = ControllScheme.Touch;
                defaultActionMap = ActionMaps.PlayerTouch;
            }
        }

        private void OnEnable()
        {
            BindEvnets();
        }

        private void Start()
        {
            actionMapNameDict.Add(ActionMaps.PlayerKeyboardMouse, "Player");
            actionMapNameDict.Add(ActionMaps.PlayerTouch, "Player");
            actionMapNameDict.Add(ActionMaps.SpawnAreaSetter, "SpawnAreaSetter");

            inputerDict.Add((playerControllerInput as IActionMapInputer).GetActionMap(), playerControllerInput);
            inputerDict.Add((spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput);

            ChangeActionMapDefault();
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
            selectableController = new SelectableController();
            playerControllerInput.Initialize(selectableController, nodeList, moveScreenJudger);
        }
        
        private void ChangeActionMapDefault()
        {
            ChangeActionMapSelected(defaultActionMap);
        }

        private void BindEvnets()
        {
            spawnAreaSetterInput.OnSetSpawnAreaFinished += ChangeActionMapDefault;
            spawnAreaSetterInput.OnSpawnUnitRequested += HandlerSpawnUnit;
            spawnAreaSetterInput.OnSetSpawnAreaFinished += HandlerSetSpawnAreaFinished;
            spawnAreaSetterInput.OnTrackMouse += HandlerTrackMouse;
            spawnAreaSetterInput.OnCancelSpawnAreaSet += HandlerCancelSpawnAreaSet;
            spawnAreaSetterInput.OnPointerNotOverGameObject += HandlerPointerNotOverGameObject;

            playerControllerInput.OnControllMenu += () => OnControllMenu?.Invoke();
            playerControllerInput.OnMoveScreen += playerController.SetVelocity;

            keyboardMouseInputMeditator.OnDirectionChanged += playerController.SetDirection;
            keyboardMouseInputMeditator.OnZoomRequest += playerController.SetTargetZoom;

            touchInputMeditator.OnDirectionChanged += playerController.SetDirection;
            touchInputMeditator.OnZoomRequest += playerController.SetTargetZoom;
        }

        private void UnbindEvents()
        {
            spawnAreaSetterInput.OnSetSpawnAreaFinished -= ChangeActionMapDefault;
            spawnAreaSetterInput.OnSpawnUnitRequested -= HandlerSpawnUnit;
            spawnAreaSetterInput.OnSetSpawnAreaFinished -= HandlerSetSpawnAreaFinished;
            spawnAreaSetterInput.OnTrackMouse -= HandlerTrackMouse;
            spawnAreaSetterInput.OnCancelSpawnAreaSet -= HandlerCancelSpawnAreaSet;
            spawnAreaSetterInput.OnPointerNotOverGameObject -= HandlerPointerNotOverGameObject;

            playerControllerInput.OnControllMenu -= () => OnControllMenu?.Invoke();
            playerControllerInput.OnMoveScreen -= playerController.SetVelocity;

            keyboardMouseInputMeditator.OnDirectionChanged -= playerController.SetDirection;
            keyboardMouseInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;

            touchInputMeditator.OnDirectionChanged -= playerController.SetDirection;
            touchInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;
        }

        public void ChangeActionMapSelected(ActionMaps actionMap)
        {
            string actionMapName = actionMapNameDict[actionMap];

            playerInputComponent.SwitchCurrentActionMap(actionMapName);

            currentInputer?.ActionMapDeactivated();

            currentInputer = inputerDict[actionMap];

            currentInputer.ActionMapActivated();
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
        private void HandlerPointerNotOverGameObject(bool value)
        {
            OnPointerNotOverGameObject?.Invoke(value);
        }
    }
}

