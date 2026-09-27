using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller
{
    public class InGameKeyboardMouseInputBase : MonoBehaviour
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action<float> OnZoomRequest;
        public event Action OnControllMenu;        

        protected UnitSelector unitSelector;
        
        protected Camera mainCamera;
        protected IEnumerator holdJudgementCoroutine;

        protected Vector2 mousePosition;
        protected bool isPointerOverGameObject;
        protected bool isDrag;

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
        
        protected void Initialize(UnitSelector unitSelector)
        {
            this.unitSelector = unitSelector;
        }

        public void OnPressESC(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnControllMenu?.Invoke();
            }
        }

        public virtual void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();
            }
        }

        public virtual void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject)  // || !isInputActive
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

        public void OnMouseScroll(InputAction.CallbackContext context)
        {
            OnZoomRequest?.Invoke(context.ReadValue<float>());
        }
    }
}
