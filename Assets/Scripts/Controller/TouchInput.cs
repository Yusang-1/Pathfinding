using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller
{
    public class TouchInput : MonoBehaviour
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;

        private UnitSelector unitSelector;

        public bool Touch0Active { get; private set; }
        public Vector2 Touch0Delta { get; private set; }
        public Vector2 Touch0Pos { get; private set; }
        public bool Touch1Active { get; private set; }
        public Vector2 Touch1Pos { get; private set; }

        private bool isPointerOverGameObject;
        private bool isInputActive;
        public bool IsDrag { get; private set; }

        private IEnumerator WaitDragCoroutine;

        private void Update()
        {
            if (!isInputActive) return;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                isPointerOverGameObject = true;
            }
            else
            {
                isPointerOverGameObject = false;
            }
        }

        public void Initialize(UnitSelector unitSelector)
        {
            this.unitSelector = unitSelector;
        }

        public void SetInputActive(bool value)
        {
            isInputActive = value;
        }

        public void OnTouch0Contact(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive)
            {
                if (!(context.canceled && IsDrag)) return;
            }

            // 버튼 형태는 ReadValueAsButton()이나 IsPressed()로 상태 확인 가능
            Touch0Active = context.ReadValueAsButton();

            if (context.started)
            {
                WaitDragCoroutine = WaitDrag(Touch0Pos);
                StartCoroutine(WaitDragCoroutine);
            }

            if (context.performed)
            {
                if (Touch0Delta.sqrMagnitude > 0.02f)
                {
                    StopCoroutine(WaitDragCoroutine);
                }
            }

            if (context.canceled)
            {
                if (WaitDragCoroutine != null)
                {
                    StopCoroutine(WaitDragCoroutine);
                }

                if (IsDrag)
                {
                    HoldCanceled();
                }
                else
                {
                    var worldPos = Camera.main.ScreenToWorldPoint(
                        new Vector3(Touch0Pos.x, Touch0Pos.y, -Camera.main.transform.position.z)
                    );
                    unitSelector.CheckPointFocused(worldPos);
                    unitSelector.SelectFocused();
                }
            }
        }

        public void OnTouch0Delta(InputAction.CallbackContext context)
        {
            Touch0Delta = context.ReadValue<Vector2>();
        }

        public void OnTouch0Position(InputAction.CallbackContext context)
        {
            Touch0Pos = context.ReadValue<Vector2>();
        }

        public void OnTouch1Contact(InputAction.CallbackContext context)
        {
            Touch1Active = context.ReadValueAsButton();
        }

        public void OnTouch1Position(InputAction.CallbackContext context)
        {
            Touch1Pos = context.ReadValue<Vector2>();
        }

        private readonly float dragGoalTime = 0.5f;
        private float dragTime = 0;
        private IEnumerator WaitDrag(Vector2 startPosition)
        {
            while (true)
            {
                if (startPosition != Touch0Pos)
                {
                    dragTime += Time.deltaTime;

                    if (dragTime >= dragGoalTime)
                    {
                        HoldStarted();
                        break;
                    }
                }
                yield return null;
            }

            while (IsDrag)
            {
                HoldPerformed();
                yield return null;
            }
        }

        private void HoldStarted()
        {
            OnHoldStarted?.Invoke(Touch0Pos);
            IsDrag = true;
        }
        private void HoldPerformed()
        {
            OnHoldPerformed?.Invoke(Touch0Pos);
        }
        private void HoldCanceled()
        {
            IsDrag = false;
            unitSelector.SelectFocused();
            OnHoldCanceled?.Invoke();
        }

        public void ActionMapActivated() => isInputActive = true;
        public void ActionMapDeactivated() => isInputActive = false;
        public bool IsActivated() => isInputActive;
    }
}
