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
        [SerializeField] private SpawnAreaSetterInput spawnAreaSetterInput;

        [SerializeField] private CreateMapKeyboardMouseInput createMapKeyboardMouseInput;

        [SerializeField] private PlayerController playerController;
        [SerializeField] private MoveScreenJudger moveScreenJudger;

        private readonly SelectableController selectableController = new();
        private IActionMapInputer currentInputer;
        private readonly KeyboardMouseInputPlayerControllerMeditator keyboardMouseInputMeditator = new();
        private TouchInputPlayerControllerMeditator touchInputMeditator;

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

            var keyboardMouseInputerDict = new Dictionary<ActionMaps, IActionMapInputer>
            {
                { (createMapKeyboardMouseInput as IActionMapInputer).GetActionMap(), createMapKeyboardMouseInput },
                { (spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput }
            };
            inputerSchemeDict.Add(ControllScheme.KeyboardMouse, keyboardMouseInputerDict);

            // var touchInputerDict = new Dictionary<ActionMaps, IActionMapInputer>
            // {
            //     { (createMapTouchInput as IActionMapInputer).GetActionMap(), createMapTouchInput },
            //     { (spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput }
            // };
            // inputerSchemeDict.Add(ControllScheme.Touch, touchInputerDict);
        }

        private void OnEnable()
        {
            BindEvnets();
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

        public void Initialize(NodeList nodeList)
        {
            if (currentScheme == ControllScheme.KeyboardMouse)
            {
                createMapKeyboardMouseInput.Initialize(selectableController, moveScreenJudger, nodeList);
                SetSchemeDict(currentScheme);
            }
            else if (currentScheme == ControllScheme.Touch)
            {

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

        private void BindEvnets()
        {
            if (isEventBound) return;
            
            spawnAreaSetterInput.OnSetSpawnAreaFinished += ChangeActionMapDefault;
            spawnAreaSetterInput.OnSpawnUnitRequested += HandlerSpawnUnit;
            spawnAreaSetterInput.OnSetSpawnAreaFinished += HandlerSetSpawnAreaFinished;
            spawnAreaSetterInput.OnTrackMouse += HandlerTrackMouse;
            spawnAreaSetterInput.OnCancelSpawnAreaSet += HandlerCancelSpawnAreaSet;
            spawnAreaSetterInput.OnPointerNotOverGameObject += HandlerPointerNotOverGameObject;

            var current = playerInputComponent.currentControlScheme;
            if (current == KEYBOARD_MOUSE)
            {
                createMapKeyboardMouseInput.OnControllMenu += HandlerControllMenu;
                createMapKeyboardMouseInput.OnMoveScreen += playerController.SetVelocity;
                createMapKeyboardMouseInput.OnActionMapInputerActivated += keyboardMouseInputMeditator.AddBind;
                createMapKeyboardMouseInput.OnActionMapInputerDeactivated += keyboardMouseInputMeditator.RemoveBind;

                keyboardMouseInputMeditator.OnZoomRequest += playerController.SetTargetZoom;
            }

            // if (current == TOUCH)
            // {

            //     touchInputMeditator.OnDirectionChanged += playerController.SetDirection;
            //     touchInputMeditator.OnZoomRequest += playerController.SetTargetZoom;
            // }
            
            isEventBound = true;
        }

        private void UnbindEvents()
        {
            if (!isEventBound) return;
            
            spawnAreaSetterInput.OnSetSpawnAreaFinished -= ChangeActionMapDefault;
            spawnAreaSetterInput.OnSpawnUnitRequested -= HandlerSpawnUnit;
            spawnAreaSetterInput.OnSetSpawnAreaFinished -= HandlerSetSpawnAreaFinished;
            spawnAreaSetterInput.OnTrackMouse -= HandlerTrackMouse;
            spawnAreaSetterInput.OnCancelSpawnAreaSet -= HandlerCancelSpawnAreaSet;
            spawnAreaSetterInput.OnPointerNotOverGameObject -= HandlerPointerNotOverGameObject;

            var current = playerInputComponent.currentControlScheme;
            if (current == KEYBOARD_MOUSE)
            {
                createMapKeyboardMouseInput.OnControllMenu -= HandlerControllMenu;
                createMapKeyboardMouseInput.OnMoveScreen -= playerController.SetVelocity;
                createMapKeyboardMouseInput.OnActionMapInputerActivated -= keyboardMouseInputMeditator.AddBind;
                createMapKeyboardMouseInput.OnActionMapInputerDeactivated -= keyboardMouseInputMeditator.RemoveBind;

                keyboardMouseInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;
            }

            // if (current == TOUCH)
            // {

            //     touchInputMeditator.OnDirectionChanged -= playerController.SetDirection;
            //     touchInputMeditator.OnZoomRequest -= playerController.SetTargetZoom;
            // }
            
            isEventBound = false;
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
        private void HandlerControllMenu()
        {
            OnControllMenu?.Invoke();
        }
    }
}

