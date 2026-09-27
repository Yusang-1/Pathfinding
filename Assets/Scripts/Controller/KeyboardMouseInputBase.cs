using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

namespace Assets.Scripts.Controller
{
    public class KeyboardMouseInputBase : MonoBehaviour
    {
        public event Action<float> OnZoomRequest;
        public event Action OnControllMenu;

        protected Camera mainCamera;

        protected Vector2 mousePosition;
        protected bool isPointerOverGameObject;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        protected virtual void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                isPointerOverGameObject = true;
            }
            else
            {
                isPointerOverGameObject = false;
            }
        }

        protected void InvokeOnControllMenu()
        {
            OnControllMenu?.Invoke();
        }

        public virtual void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();
            }
        }

        public void OnMouseScroll(InputAction.CallbackContext context)
        {
            OnZoomRequest?.Invoke(context.ReadValue<float>());
        }
    }
}
