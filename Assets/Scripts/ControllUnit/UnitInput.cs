using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class UnitInput : MonoBehaviour, IActionMapInputer
    {
        public event Action<Vector2> OnDirectionChanged;
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPreformed;
        public event Action OnHoldCanceled;
        public event Action OnControllMenu;

        private SelectableController selectableController;

        private readonly Dictionary<int, Vector2> directionDict = new();

        [SerializeField] private string actionMapName;
        private Vector2 sumOfDirection;
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

        public void Initialize(SelectableController selectableController)
        {
            this.selectableController = selectableController;
        }

        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive)
            {
                if(!(context.canceled && isHold)) return;                
            }

            if (context.started)
            {
                StartCoroutine(HoldJudger());
            }

            if (context.canceled)
            {
                isClickCanceled = true;

                if (isHold)
                {
                    HoldCanceled();
                }
                else
                {
                    if (isShiftPressed)
                    {
                        selectableController.ShiftSelected();
                    }
                    else
                    {
                        selectableController.Selected();
                    }
                }
            }
        }

        [SerializeField] private float holdJudgeLimit;
        private float holdJudgeTime;
        private bool isHold;
        private bool isClickCanceled;

        /// <summary>
        /// holdJudgeLimit 만큼의 시간이 지나면 hold 판정
        /// </summary>
        private IEnumerator HoldJudger()
        {
            holdJudgeTime = 0;
            isHold = false;
            isClickCanceled = false;

            while (holdJudgeTime < holdJudgeLimit)
            {
                if (isClickCanceled)
                {
                    break;
                }
                holdJudgeTime += Time.deltaTime;
                yield return null;
            }

            if (!isClickCanceled)
            {
                HoldStarted();
            }

            while (!isClickCanceled)
            {
                HoldPerformed();
                yield return null;
            }
        }
        private void HoldStarted()
        {
            OnHoldStarted?.Invoke(mousePosition);
            isHold = true;
        }
        private void HoldPerformed()
        {
            OnHoldPreformed?.Invoke(mousePosition);
        }
        private void HoldCanceled()
        {
            if(isShiftPressed)
            {
                selectableController.ShiftSelectedList();
            }
            else
            {
                selectableController.Selected();                
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
                    selectableController.ShiftRightClickMove(worldPos);
                }
                else
                {
                    selectableController.RightClickMove(worldPos);
                }
            }
        }

        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (!isInputActive) return;

            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();

                if (isHold || isPointerOverGameObject) return;

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.z));
                Vector3 origin = Camera.main.transform.position;
                Vector3 direction = -(worldPos - origin).normalized;

                if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
                {
                    if (hit.collider.TryGetComponent<ISelectableUnit>(out ISelectableUnit selectable))
                    {
                        selectableController.UnitFocusedPoint(selectable);
                    }
                    else
                        selectableController.UnitFocusedPoint(null);
                }
                else
                {
                    selectableController.UnitFocusedPoint(null);
                }
            }
        }

        public void OnMoveForward(InputAction.CallbackContext context)
        {
            HandleKeyInput('w', context);
        }

        public void OnMoveBackward(InputAction.CallbackContext context)
        {
            HandleKeyInput('s', context);
        }

        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            HandleKeyInput('a', context);
        }

        public void OnMoveRight(InputAction.CallbackContext context)
        {
            HandleKeyInput('d', context);
        }

        private void HandleKeyInput(int index, InputAction.CallbackContext context)
        {
            if (!isInputActive) return;

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

        private void SetDirection(Vector2 dir)
        {
            sumOfDirection += dir;
            OnDirectionChanged?.Invoke(sumOfDirection.normalized);
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

        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            if (!isInputActive) return;

            float scrollY = context.ReadValue<float>();

            if (context.started)
            {
                if (scrollY != 0)
                {
                    Vector3 pos = Camera.main.transform.position;
                    pos.z += scrollY;
                    Camera.main.transform.position = pos;
                }
            }
        }
        
        public void OnMenu(InputAction.CallbackContext context)
        {
            if(context.started)
            {
                OnControllMenu?.Invoke();                
            }
        }

        public string GetActionMapName() => actionMapName;
        public void ActionMapActivated() => isInputActive = true;
        public void ActionMapDeactivated() => isInputActive = false;
    }
}
