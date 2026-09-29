using UnityEngine;
using System;

namespace Assets.Scripts.Controller
{
    public class TouchInputPlayerControllerMeditator
    {
        public event Action<float> OnZoomRequest;

        private TouchInputBase touchInput;
        
        private readonly float touchPinchSensitivity = 0.05f;
        private float previousTouchDistance;
        private float targetZoom;
        private bool isTouchZooming = false;

        public void SetTouchInput(TouchInputBase touchInput)
        {            
            this.touchInput = touchInput;

            var targetCamera = Camera.main;
            targetZoom = targetCamera.orthographic
                ? targetCamera.orthographicSize
                : targetCamera.fieldOfView;
        }

        public void Update()
        {
            HandleTouchPinch();
        }

        public void HandleTouchPinch()
        {            
            if (touchInput.Touch0Active && touchInput.Touch1Active)
            {
                float currentTouchDistance = Vector2.SqrMagnitude(touchInput.Touch0Pos - touchInput.Touch1Pos);

                if (!isTouchZooming)
                {
                    previousTouchDistance = currentTouchDistance;
                    isTouchZooming = true;
                }
                else
                {
                    float distanceDelta = previousTouchDistance - currentTouchDistance;
                    targetZoom += distanceDelta * touchPinchSensitivity;

                    previousTouchDistance = currentTouchDistance;

                    OnZoomRequest?.Invoke(targetZoom);
                }
            }
            else
            {
                isTouchZooming = false;
            }
        }
    }
}

