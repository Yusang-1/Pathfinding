using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections;

namespace Assets.Scripts.ControllUnit
{
    public class UnitInput : MonoBehaviour, IActionMapInputer
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action OnControllMenu;

        private UnitSelector unitSelector;

        [SerializeField] private ActionMaps actionMap;

        private Vector2 mousePosition;
        private bool isPointerOverGameObject;
        private bool isShiftPressed;
        private bool isInputActive;

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

        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive)
            {
                if (!(context.canceled && isDrag)) return;
            }

            if (context.started)
            {
                WaitDragCoroutine = WaitDrag(mousePosition);
                StartCoroutine(WaitDragCoroutine);
            }

            if (context.canceled)
            {
                StopCoroutine(WaitDragCoroutine);

                if (isDrag)
                {
                    HoldCanceled();
                }
                else
                {
                    if (isShiftPressed)
                    {                        
                        unitSelector.ShiftSelectedFocused();
                    }
                    else
                    {
                        unitSelector.SelectFocused();
                    }
                }
            }
        }

        private bool isDrag;
        private IEnumerator WaitDragCoroutine;
        private IEnumerator WaitDrag(Vector2 startPosition)
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

            if (isShiftPressed)
            {                
                unitSelector.ShiftSelectedFocusedList();
            }
            else
            {
                unitSelector.SelectFocused();
            }
            OnHoldCanceled?.Invoke();
        }

        // public event Action<Vector3> OnRightClickRequested;        
        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive) return;

            if (context.canceled)
            {
                Debug.Log("Right Click");
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));

                if (isShiftPressed)
                {
                    unitSelector.ShiftRightClickMove(worldPos);
                }
                else
                {
                    unitSelector.RightClickMove(worldPos);
                }
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

        public void OnPressShift(InputAction.CallbackContext context)
        {
            if (!isInputActive) return;

            if (context.started)
            {
                isShiftPressed = true;

            }

            if (context.canceled)
            {
                isShiftPressed = false;

            }
        }

        public void OnMenu(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnControllMenu?.Invoke();
            }
        }

        public ActionMaps GetActionMap() => actionMap;
        public void ActionMapActivated() => isInputActive = true;
        public void ActionMapDeactivated() => isInputActive = false;
    }
}
