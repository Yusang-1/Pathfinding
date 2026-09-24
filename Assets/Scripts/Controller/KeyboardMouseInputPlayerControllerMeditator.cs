using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Controller
{
    public class KeyboardMouseInputPlayerControllerMeditator
    {
        public event Action<Vector2> OnDirectionChanged;
        public event Action<float> OnZoomRequest;

        private readonly Dictionary<int, Vector2> moveKeyVectorDict = new();

        private readonly float mouseScrollSensitivity = 1f;

        private Vector2 totalDirection;

        public KeyboardMouseInputPlayerControllerMeditator(KeyboardMouseInput keyboardMouseInput)
        {
            keyboardMouseInput.OnAddDirection += AddDirection;
            keyboardMouseInput.OnRemoveDirection += RemoveDirection;
            keyboardMouseInput.OnZoomRequest += HandleMouseScroll;
        }

        private void AddDirection(int key, Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                return;
            }

            if (!moveKeyVectorDict.ContainsKey(key))
            {
                moveKeyVectorDict.Add(key, direction);
            }

            totalDirection += direction;

            OnDirectionChanged?.Invoke(totalDirection.normalized);
        }

        private void RemoveDirection(int key)
        {
            if (!moveKeyVectorDict.ContainsKey(key))
            {
                return;
            }

            totalDirection -= moveKeyVectorDict[key];

            OnDirectionChanged?.Invoke(totalDirection.normalized);
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
