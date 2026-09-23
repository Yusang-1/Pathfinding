using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSUnitInput : MonoBehaviour, IActionMapInputer
    {
        public event Action<Vector3> OnHoldStarted;
        public event Func<Vector3, Vector3?> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action OnControllMenu;

        private ECSSelectableController selectableController;

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

        public void Initialize(ECSSelectableController selectableController)
        {
            this.selectableController = selectableController;
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
                    selectableController.MakeSelectionRequest(mouseWorldPosition, isShiftPressed);
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

        private Vector3? holdPerformedWorldPosition;
        private void HoldStarted()
        {
            OnHoldStarted?.Invoke(mousePosition);
            isDrag = true;
        }
        private void HoldPerformed()
        {
            holdPerformedWorldPosition = OnHoldPerformed?.Invoke(mousePosition);
            if (holdPerformedWorldPosition == null) return;

            Vector3 position = (Vector3)holdPerformedWorldPosition;
            selectableController.CheckUnitsInArea(mouseWorldPosition, position);
        }

        private void HoldCanceled()
        {
            isDrag = false;
            if (holdPerformedWorldPosition == null) return;

            Vector3 position = (Vector3)holdPerformedWorldPosition;
            selectableController.MakeSelectionRequest(mouseWorldPosition, isShiftPressed);
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

                selectableController.MakeMoveCommand(mouseWorldPosition, isShiftPressed);
            }
        }

        private Vector3 mouseWorldPosition;
        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (!isInputActive) return;

            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();

                if (isDrag || isPointerOverGameObject) return;

                mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
                
                // 마우스 커서 아래 유닛이 있는지 확인
                selectableController.CheckUnitIsBelowMouse(mouseWorldPosition);
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
