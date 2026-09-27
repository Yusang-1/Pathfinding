using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller
{
    public class InGameTouchInputBase : TouchInputBase
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;

        private UnitSelector unitSelector;

        private IEnumerator WaitDragCoroutine;        

        public void Initialize(UnitSelector unitSelector)
        {
            this.unitSelector = unitSelector;
        }

        public override void OnTouch0Contact(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject)
            {
                if (!(context.canceled && IsDrag)) return;
            }

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
    }
}
