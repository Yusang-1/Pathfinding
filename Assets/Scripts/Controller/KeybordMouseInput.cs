using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Assets.Scripts.Controller
{
    public class KeyboardMouseInput : MonoBehaviour
    {
        public event Action<int, Vector2> OnAddDirection;
        public event Action<int> OnRemoveDirection;
        public event Action<float> OnZoomRequest;
                                
        public void OnPressW(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAddDirection?.Invoke('w', context.ReadValue<Vector2>());
            }

            if (context.canceled)
            {
                OnRemoveDirection?.Invoke('w');
            }
        }

        public void OnPressA(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAddDirection?.Invoke('a', context.ReadValue<Vector2>());
            }

            if (context.canceled)
            {
                OnRemoveDirection?.Invoke('a');
            }
        }

        public void OnPressS(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAddDirection?.Invoke('s', context.ReadValue<Vector2>());
            }

            if (context.canceled)
            {
                OnRemoveDirection?.Invoke('s');
            }
        }

        public void OnPressD(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAddDirection?.Invoke('d', context.ReadValue<Vector2>());
            }

            if (context.canceled)
            {
                OnRemoveDirection?.Invoke('d');
            }
        }

        public void OnMouseScroll(InputAction.CallbackContext context)
        {            
            OnZoomRequest?.Invoke(context.ReadValue<float>());
        }                
    }
}
