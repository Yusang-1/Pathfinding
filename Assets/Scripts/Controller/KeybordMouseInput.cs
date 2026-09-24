using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller
{
    public class KeyboardMouseInput : MonoBehaviour
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action<int, Vector2> OnAddDirection;
        public event Action<int> OnRemoveDirection;
        public event Action<float> OnZoomRequest;
        public event Action OnControllMenu;

        private UnitSelector unitSelector;

        private Vector2 mousePosition;
        private bool isPointerOverGameObject;
        private bool isInputActive;
        private bool isDrag;

        private IEnumerator holdJudgementCoroutine;

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

        public void OnPressW(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAddDirection?.Invoke('w', context.ReadValue<Vector2>());
            }

            if (context.canceled)
            {
                OnRemoveDirection?.Invoke('w');
            }
        }

        public void OnPressA(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAddDirection?.Invoke('a', context.ReadValue<Vector2>());
            }

            if (context.canceled)
            {
                OnRemoveDirection?.Invoke('a');
            }
        }

        public void OnPressS(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAddDirection?.Invoke('s', context.ReadValue<Vector2>());
            }

            if (context.canceled)
            {
                OnRemoveDirection?.Invoke('s');
            }
        }

        public void OnPressD(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnAddDirection?.Invoke('d', context.ReadValue<Vector2>());
            }

            if (context.canceled)
            {
                OnRemoveDirection?.Invoke('d');
            }
        }

        public void OnPressESC(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnControllMenu?.Invoke();
            }
        }

        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (!isInputActive) return;

            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();

                if (isDrag || isPointerOverGameObject) return;

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));

                unitSelector.CheckPointFocused(worldPos);
            }
        }

        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive)
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
        private void HoldCanceled()
        {
            isDrag = false;
            unitSelector.SelectFocused();
            OnHoldCanceled?.Invoke();
        }

        private IEnumerator HoldJudgement(Vector2 startPosition)
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
        
        public void ActionMapActivated() => isInputActive = true;
        public void ActionMapDeactivated() => isInputActive = false;
        public bool IsActivated() => isInputActive;
    }
}
