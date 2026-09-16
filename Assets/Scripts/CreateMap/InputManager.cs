using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.CreateMap
{
    public class InputManager : MonoBehaviour
    {
        public event Action<Vector3> OnSetSpawnAreaRequested;
        public event Action OnControllMenu;
        
        public event Action<Vector3> OnSpawnUnitRequested;
        public event Action OnSetSpawnAreaFinished;

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInputComponent;
        [SerializeField] private PlayerControllInput playerControllerInput;        
        [SerializeField] private SpawnAreaSetterInput spawnAreaSetterInput;

        private SelectableController selectableController;

        private IActionMapInputer currentInputer;

        private readonly Dictionary<ActionMaps, string> actionMapNameDict = new();
        private readonly Dictionary<ActionMaps, IActionMapInputer> inputerDict = new();

        private void OnEnable()
        {
            BindEvnets();
        }

        private void Start()
        {
            actionMapNameDict.Add(ActionMaps.Player, "Player");            
            actionMapNameDict.Add(ActionMaps.SpawnAreaSetter, "SpawnAreaSetter");
            
            inputerDict.Add((playerControllerInput as IActionMapInputer).GetActionMap(), playerControllerInput);            
            inputerDict.Add((spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput);
            
            ChangeActionMapDefault();
            
            selectableController = new SelectableController();

            playerControllerInput.Initialize(selectableController);
            playerControllerInput.OnControllMenu += () => OnControllMenu?.Invoke();
        }

        private void BindEvnets()
        {
            spawnAreaSetterInput.OnSetSpawnAreaRequested += HandlerSetSpawnAreaRequested;
            spawnAreaSetterInput.OnSetSpawnAreaFinished += ChangeActionMapDefault;
            
            spawnAreaSetterInput.OnSpawnUnitRequested += HandlerSpawnUnit;
            spawnAreaSetterInput.OnSetSpawnAreaFinished += HandlerSetSpawnAreaFinished;
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

        private void HandlerSetSpawnAreaRequested(Vector3 vec)
        {
            OnSetSpawnAreaRequested?.Invoke(vec);
        }
        
        private void HandlerSpawnUnit(Vector3 pos)
        {
            OnSpawnUnitRequested?.Invoke(pos);
        }
        
        private void HandlerSetSpawnAreaFinished()
        {
            OnSetSpawnAreaFinished?.Invoke();
        }
    }
}

