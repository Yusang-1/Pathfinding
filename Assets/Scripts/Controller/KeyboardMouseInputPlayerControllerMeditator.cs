using UnityEngine;
using System;

namespace Assets.Scripts.Controller
{
    public class KeyboardMouseInputPlayerControllerMeditator
    {
        public event Action<float> OnZoomRequest;

        private readonly float mouseScrollSensitivity = 1f;

        public void AddBind(KeyboardMouseInputBase keyboardMouseInputBase)
        {
            keyboardMouseInputBase.OnZoomRequest += HandleMouseScroll;
        }
        
        public void RemoveBind(KeyboardMouseInputBase keyboardMouseInputBase)
        {
            keyboardMouseInputBase.OnZoomRequest -= HandleMouseScroll;
        }

        private void HandleMouseScroll(float scrollValue)
        {
            if (scrollValue == 0f)
            {
                return;
            }

            float zoomDelta = -Mathf.Sign(scrollValue) * mouseScrollSensitivity;

            OnZoomRequest?.Invoke(zoomDelta);
        }
    }
}
