using UnityEngine;
using UnityEngine.InputSystem;

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
            
            // playerControllInput.OnSelectedCallback += ChangeActionMapSelected;
        }
        
        private void ChangeActionMapSelected(string value)
        {
            playerInputComponent.SwitchCurrentActionMap(value);
        }
        
        private void ChangeActionMapDefault()
        {
            playerInputComponent.SwitchCurrentActionMap("Player");
        }
    }
}

