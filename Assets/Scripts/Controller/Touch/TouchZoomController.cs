using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Controller.Touch
{
    public class TouchZoomController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 20f;
        
        [Header("Sensitivity Settings")]
        [SerializeField] private float touchPinchSensitivity = 0.05f;
        [SerializeField] private float smoothTime = 0.15f;

        private float targetZoom;
        private float currentZoomVelocity;
        private Vector2 touch0Pos;
        private Vector2 touch1Pos;
        private bool touch0Active;
        private bool touch1Active;
        private float previousTouchDistance;
        private bool isTouchZooming = false;

        private void Update()
        {
            HandleTouchPinch();
            ApplySmoothZoom();
        }

        public void OnTouch0Position(InputAction.CallbackContext context)
        {
            touch0Pos = context.ReadValue<Vector2>();
        }

        public void OnTouch1Position(InputAction.CallbackContext context)
        {
            touch1Pos = context.ReadValue<Vector2>();
        }

        public void OnTouch0Contact(InputAction.CallbackContext context)
        {
            // 버튼 형태는 ReadValueAsButton()이나 IsPressed()로 상태 확인 가능
            touch0Active = context.ReadValueAsButton();
        }

        public void OnTouch1Contact(InputAction.CallbackContext context)
        {
            touch1Active = context.ReadValueAsButton();
        }

        private void HandleTouchPinch()
        {
            if (touch0Active && touch1Active)
            {
                float currentTouchDistance = Vector2.SqrMagnitude(touch0Pos - touch1Pos);

                if (!isTouchZooming)
                {
                    previousTouchDistance = currentTouchDistance;
                    isTouchZooming = true;
                }
                else
                {
                    float distanceDelta = previousTouchDistance - currentTouchDistance;
                    targetZoom += distanceDelta * touchPinchSensitivity;
                    targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

                    previousTouchDistance = currentTouchDistance;
                }
            }
            else
            {
                isTouchZooming = false;
            }
        }

        private void ApplySmoothZoom()
        {
            if (targetCamera.orthographic)
            {
                targetCamera.orthographicSize = Mathf.SmoothDamp(
                    targetCamera.orthographicSize, targetZoom, ref currentZoomVelocity, smoothTime);
            }
            else
            {
                targetCamera.fieldOfView = Mathf.SmoothDamp(
                    targetCamera.fieldOfView, targetZoom, ref currentZoomVelocity, smoothTime);
            }
        }
    }
}

