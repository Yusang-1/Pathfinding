using System;
using UnityEngine;

namespace Assets.Scripts.CreateMap
{
    public class InputManager : MonoBehaviour
    {
        public event Action OnControllMenu;
        
        private PlayerControllInput playerInput;        
        private SelectableController selectableController;

        private void Start()
        {
            selectableController = new SelectableController();
            
            playerInput = GetComponent<PlayerControllInput>();
            playerInput.Initialize(selectableController);
            playerInput.OnControllMenu += () => OnControllMenu?.Invoke();
        }
    }
}

