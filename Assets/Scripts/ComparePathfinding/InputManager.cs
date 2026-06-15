using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private InputActionMap actionMap;

    private void Awake()
    {        
        actionMap = inputActions.actionMaps[0];
        actionMap.Enable();
    }    
}
