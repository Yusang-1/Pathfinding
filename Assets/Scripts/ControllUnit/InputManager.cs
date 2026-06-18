using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class InputManager : MonoBehaviour
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPreformed;
        public event Func<HashSet<ISelectableUnit>> OnHoldCanceled;
        
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInputComponent;
        [SerializeField] private PlayerControllInput playerInput;
        [SerializeField] private UnitInput unitInput;

        private InputActionMap actionMap;
        private readonly SelectableController selectableController;

        private IActionMapInputer currentInputer;
        private readonly Dictionary<string, IActionMapInputer> inputerDict = new();

        private void Awake()
        {
            actionMap = inputActions.actionMaps[0];
            actionMap.Enable();
        }

        private void Start()
        {
            inputerDict.Add((playerInput as IActionMapInputer).GetActionMapName(), playerInput);
            inputerDict.Add((unitInput as IActionMapInputer).GetActionMapName(), unitInput);
            
            playerInput.OnHoldStarted += (vec) => OnHoldStarted?.Invoke(vec);
            playerInput.OnHoldPreformed += (vec) => OnHoldPreformed?.Invoke(vec);
            playerInput.OnHoldCanceled += () => OnHoldCanceled?.Invoke();
            
            unitInput.OnHoldStarted += (vec) => OnHoldStarted?.Invoke(vec);
            unitInput.OnHoldPreformed += (vec) => OnHoldPreformed?.Invoke(vec);
            unitInput.OnHoldCanceled += () => OnHoldCanceled?.Invoke();
            
            ChangeActionMapDefault();
        }
        
        public void Initialize(SelectableController selectableController)
        {
            selectableController.GetActions(ChangeActionMapSelected, ChangeActionMapDefault);

            playerInput.Initialize(selectableController);
            unitInput.Initialize(selectableController);
        }

        private void ChangeActionMapSelected(string value)
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
    }
}

public interface IActionMapInputer
{
    public string GetActionMapName();
    public void ActionMapActivated();
    public void ActionMapDeactivated();
}

