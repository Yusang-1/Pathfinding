using UnityEngine;

namespace Assets.Scripts.CreateMap
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerControllInput playerInput;
        
        private SelectableController selectableController;

        private void Start()
        {
            selectableController = new SelectableController();
            
            playerInput.Initialize(selectableController);
        }
    }
}

