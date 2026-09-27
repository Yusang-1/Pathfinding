using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller
{
    public class InGameKeyboardMouseInputBase : KeyboardMouseInputBase
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;

        protected UnitSelector unitSelector;

        protected IEnumerator holdJudgementCoroutine;

        protected bool isDrag;

        protected void Initialize(UnitSelector unitSelector)
        {
            this.unitSelector = unitSelector;
        }

        public void OnPressESC(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                InvokeOnControllMenu();
            }
        }        

        public virtual void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject)
            {
                if (!(context.canceled && isDrag)) return;
            }

            if (context.started)
            {
                holdJudgementCoroutine = HoldJudgement(mousePosition);
                StartCoroutine(holdJudgementCoroutine);
            }

            if (context.canceled)
            {
                StopCoroutine(holdJudgementCoroutine);

                if (isDrag)
                {
                    HoldCanceled();
                }
                else
                {
                    unitSelector.SelectFocused();
                }
            }
        }

        private void HoldStarted()
        {
            OnHoldStarted?.Invoke(mousePosition);
            isDrag = true;
        }
        private void HoldPerformed()
        {
            OnHoldPerformed?.Invoke(mousePosition);
        }
        protected virtual void HoldCanceled()
        {
            isDrag = false;
            unitSelector.SelectFocused();
            OnHoldCanceled?.Invoke();
        }
        protected void InvokeOnHoldCanceled()
        {
            OnHoldCanceled?.Invoke();
        }

        protected IEnumerator HoldJudgement(Vector2 startPosition)
        {
            while (true)
            {
                if (startPosition != mousePosition)
                {
                    HoldStarted();
                    break;
                }
                yield return null;
            }

            while (isDrag)
            {
                HoldPerformed();
                yield return null;
            }
        }
    }
}
