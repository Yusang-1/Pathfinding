using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.ControllUnit
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private PlayerControllInput playerControllInput;
        
        private InputActionMap actionMap;

        private void Awake()
        {
            actionMap = inputActions.actionMaps[0];
            actionMap.Enable();
        }

        private void Start()
        {
            playerControllInput.OnSelectedCallback += ChangeActionMapSelected;
        }
        
        private void ChangeActionMapSelected(string value)
        {
            playerInput.SwitchCurrentActionMap(value);
        }
    }
}

