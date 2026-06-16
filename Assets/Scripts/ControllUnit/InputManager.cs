using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInputComponent;
        [SerializeField] private PlayerControllInput playerInput;
        [SerializeField] private UnitInput unitInput;

        private InputActionMap actionMap;
        private SelectableController selectableController;

        private IActionMapInputer currentInputer;
        private readonly Dictionary<string, IActionMapInputer> inputerDict = new();

        private void Awake()
        {
            actionMap = inputActions.actionMaps[0];
            actionMap.Enable();
        }

        private void Start()
        {
            selectableController = new SelectableController(ChangeActionMapSelected, ChangeActionMapDefault);

            playerInput.Initialize(selectableController);
            unitInput.Initialize(selectableController);

            inputerDict.Add((playerInput as IActionMapInputer).GetActionMapName(), playerInput);
            inputerDict.Add((unitInput as IActionMapInputer).GetActionMapName(), unitInput);
            
            ChangeActionMapDefault();
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
            playerInputComponent.SwitchCurrentActionMap(DefaultActionMapName);
        }
    }
}

public interface IActionMapInputer
{
    public string GetActionMapName();
    public void ActionMapActivated();
    public void ActionMapDeactivated();
}

