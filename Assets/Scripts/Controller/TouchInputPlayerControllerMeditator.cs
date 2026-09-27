using UnityEngine;
using System;

namespace Assets.Scripts.Controller
{
    public class TouchInputPlayerControllerMeditator
    {
        public event Action<Vector2> OnDirectionChanged;
        public event Action<float> OnZoomRequest;

        private readonly TouchInput touchInput;

        private Vector2 lastMoveDirection;
        private Vector2 totalDirection;
        private readonly float touchPinchSensitivity = 0.05f;
        private float previousTouchDistance;
        private float targetZoom;
        private bool isTouchZooming = false;

        public TouchInputPlayerControllerMeditator(TouchInput touchInput)
        {
            this.touchInput = touchInput;

            var targetCamera = Camera.main;
            targetZoom = targetCamera.orthographic
                ? targetCamera.orthographicSize
                : targetCamera.fieldOfView;
        }             

        public void Update()
        {
            HandleTouchMove();
            HandleTouchPinch();
        }

        public void HandleTouchMove()
        {
            // 두 손가락이 닿아있을 때는 '줌' 모드이므로, 
            // 한 손가락만 정확히 화면에 붙어있고 터치 0번이 활성화되어 있을 때만 '이동' 처리합니다.
            if (touchInput.Touch0Active && !touchInput.Touch1Active && !touchInput.IsDrag)
            {
                // touch0Delta 값이 존재할 때만 카메라 이동
                if (touchInput.Touch0Delta.sqrMagnitude > 0.02f)
                {
                    // 드래그 방향과 카메라 이동 방향을 맞추기 위해 음수(-)를 곱
                    var direction = -touchInput.Touch0Delta;

                    // 이전 프레임의 direction 초기화
                    totalDirection -= lastMoveDirection;

                    totalDirection += direction;
                    lastMoveDirection = direction;

                    OnDirectionChanged?.Invoke(totalDirection);
                }
                else
                {
                    totalDirection -= lastMoveDirection;
                    lastMoveDirection = Vector2.zero;

                    OnDirectionChanged?.Invoke(totalDirection);
                }
            }
            else
            {
                totalDirection -= lastMoveDirection;
                lastMoveDirection = Vector2.zero;

                OnDirectionChanged?.Invoke(totalDirection);
            }
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

