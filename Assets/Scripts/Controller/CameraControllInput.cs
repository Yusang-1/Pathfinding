using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Controller
{
    public class CameraControllInput : MonoBehaviour
    {
        public event Action<Vector2> OnDirectionChanged;

        private readonly Dictionary<int, Vector2> directionDict = new();

        [Header("Camera Settings")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float minZoom = 30f;
        [SerializeField] private float maxZoom = 80f;

        [Header("Sensitivity Settings")]
        [SerializeField] private float mouseScrollSensitivity = 1f;
        [SerializeField] private float touchPinchSensitivity = 0.05f;
        [SerializeField] private float smoothTime = 0.15f;

        [Header("Move Settings")]
        [SerializeField] private float moveSpeed = 0.5f; // 이동 감도
        private Vector2 touch0Delta;

        private float targetZoom;
        private float currentZoomVelocity;

        private Vector2 sumOfDirection;
        private float mouseScrollValue;
        private Vector2 touch0Pos;
        private Vector2 touch1Pos;
        private bool touch0Active;
        private bool touch1Active;

        private float previousTouchDistance;
        private bool isTouchZooming = false;

        private void Start()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            targetZoom = targetCamera.orthographic
                ? targetCamera.orthographicSize
                : targetCamera.fieldOfView;
        }

        private void Update()
        {
            HandleTouchMove();
            HandleMouseScroll();
            HandleTouchPinch();
            ApplySmoothZoom();
        }

        public void OnPressW(InputAction.CallbackContext context)
        {
            HandleKeyInput('w', context);
        }

        public void OnPressS(InputAction.CallbackContext context)
        {
            HandleKeyInput('s', context);
        }

        public void OnPressA(InputAction.CallbackContext context)
        {
            HandleKeyInput('a', context);
        }

        public void OnPressD(InputAction.CallbackContext context)
        {
            HandleKeyInput('d', context);
        }

        public void OnTouch0Delta(InputAction.CallbackContext context)
        {
            touch0Delta = context.ReadValue<Vector2>();
        }

        public void OnMouseScroll(InputAction.CallbackContext context)
        {
            mouseScrollValue = context.ReadValue<float>();
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

        private void HandleKeyInput(int index, InputAction.CallbackContext context)
        {
            // if (!isInputActive) return;

            if (context.started)
            {
                Vector2 value = context.ReadValue<Vector2>();
                directionDict[index] = value;
                SetDirection(value);
            }

            if (context.canceled)
            {
                Vector2 value = directionDict[index];
                SetDirection(-value);
            }
        }

        private void HandleTouchMove()
        {
            // 두 손가락이 닿아있을 때는 '줌' 모드이므로, 
            // 한 손가락만 정확히 화면에 붙어있고 터치 0번이 활성화되어 있을 때만 '이동' 처리합니다.
            if (touch0Active && !touch1Active)
            {
                // touch0Delta 값이 존재할 때만 카메라 이동
                if (touch0Delta.sqrMagnitude > 0.01f)
                {
                    // 드래그 방향과 카메라 이동 방향을 맞추기 위해 음수(-)를 곱해줍니다.
                    // (화면을 왼쪽으로 밀면 카메라가 오른쪽으로 이동해야 화면이 밀리는 느낌이 듭니다)
                    float moveX = -touch0Delta.x * moveSpeed * Time.deltaTime;
                    float moveY = -touch0Delta.y * moveSpeed * Time.deltaTime;

                    // 2D 탑다운 또는 3D 쿼터뷰 카메라 이동 예시 (프로젝트 시점에 맞게 수정)
                    transform.Translate(new Vector3(moveX, 0, moveY), Space.World);

                    // 일회성 입력이므로 계산 후 초기화
                    touch0Delta = Vector2.zero;
                }
            }
        }

        private void SetDirection(Vector2 dir)
        {
            sumOfDirection += dir;
            OnDirectionChanged(sumOfDirection.normalized);
        }

        private void HandleMouseScroll()
        {
            if (mouseScrollValue != 0)
            {
                float zoomDelta = -mouseScrollValue * mouseScrollSensitivity;
                targetZoom += zoomDelta;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

                // 이벤트 방식은 휠을 멈춰도 마지막 값이 변수에 남아있으므로 계산 후 0으로 리셋
                mouseScrollValue = 0;
            }
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
