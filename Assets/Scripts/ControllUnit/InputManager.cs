using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class InputManager : MonoBehaviour
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action OnControllMenu;
        public event Action<Vector3> OnSetSpawnAreaRequested;

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInputComponent;
        [SerializeField] private PlayerControllInput playerControllerInput;
        [SerializeField] private UnitInput unitInput;
        [SerializeField] private SpawnAreaSetterInput spawnAreaSetterInput;

        private InputActionMap actionMap;

        private IActionMapInputer currentInputer;
        private readonly Dictionary<string, IActionMapInputer> inputerDict = new();

        private void Awake()
        {
            actionMap = inputActions.actionMaps[0];
            actionMap.Enable();
        }

        private void OnEnable()
        {
            BindEvents();
        }

        private void Start()
        {
            inputerDict.Add((playerControllerInput as IActionMapInputer).GetActionMapName(), playerControllerInput);
            inputerDict.Add((unitInput as IActionMapInputer).GetActionMapName(), unitInput);
            inputerDict.Add((spawnAreaSetterInput as IActionMapInputer).GetActionMapName(), spawnAreaSetterInput);

            ChangeActionMapDefault();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        public void Initialize(SelectableController selectableController)
        {
            selectableController.GetActions(ChangeActionMapSelected, ChangeActionMapDefault);

            playerControllerInput.Initialize(selectableController);
            unitInput.Initialize(selectableController);
        }

        public void ChangeActionMapSelected(string value)
        {
            playerInputComponent.SwitchCurrentActionMap(value);

            currentInputer?.ActionMapDeactivated();

            currentInputer = inputerDict[value];

            currentInputer.ActionMapActivated();
        }

        private const string DefaultActionMapName = "Player";
        private void ChangeActionMapDefault()
        {
            ChangeActionMapSelected(DefaultActionMapName);
        }

        private void BindEvents()
        {
            playerControllerInput.OnHoldStarted += HandlerHoldStarted;
            playerControllerInput.OnHoldPerformed += HandlerHoldPerformed;
            playerControllerInput.OnHoldCanceled += HandlerHoldCanceled;
            playerControllerInput.OnControllMenu += HandlerControllMenu;

            unitInput.OnHoldStarted += HandlerHoldStarted;
            unitInput.OnHoldPerformed += HandlerHoldPerformed;
            unitInput.OnHoldCanceled += HandlerHoldCanceled;
            unitInput.OnControllMenu += HandlerControllMenu;

            spawnAreaSetterInput.OnSetSpawnAreaRequested += HandlerSetSpawnAreaRequested;
            spawnAreaSetterInput.OnSetSpawnAreaFinished += ChangeActionMapDefault;
        }

        private void UnbindEvents()
        {
            playerControllerInput.OnHoldStarted -= HandlerHoldStarted;
            playerControllerInput.OnHoldPerformed -= HandlerHoldPerformed;
            playerControllerInput.OnHoldCanceled -= HandlerHoldCanceled;
            playerControllerInput.OnControllMenu -= HandlerControllMenu;

            unitInput.OnHoldStarted -= HandlerHoldStarted;
            unitInput.OnHoldPerformed -= HandlerHoldPerformed;
            unitInput.OnHoldCanceled -= HandlerHoldCanceled;
            unitInput.OnControllMenu -= HandlerControllMenu;

            spawnAreaSetterInput.OnSetSpawnAreaRequested -= HandlerSetSpawnAreaRequested;
            spawnAreaSetterInput.OnSetSpawnAreaFinished -= ChangeActionMapDefault;
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
        private void HandlerSetSpawnAreaRequested(Vector3 vec)
        {
            OnSetSpawnAreaRequested?.Invoke(vec);
        }
    }
}

public interface IActionMapInputer
{
    public string GetActionMapName();
    public void ActionMapActivated();
    public void ActionMapDeactivated();
}

