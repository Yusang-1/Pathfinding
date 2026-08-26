using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSPlayerControllerInput : MonoBehaviour
    {
        public event Action<Vector2> OnDirectionChanged;
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action OnControllMenu;

        private ECSSelectableController selectableController;

        private readonly Dictionary<int, Vector2> directionDict = new();

        [SerializeField] private string actionMapName;
        private Vector2 sumOfDirection;
        private Vector2 mousePosition;
        private bool isPointerOverGameObject;
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
                    selectableController.MakeSelectionRequest(mouseWorldPosition, false);
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
            selectableController.MakeAreaSelectionRequest();
            OnHoldCanceled?.Invoke();
        }
        
        private Vector3 mouseWorldPosition;
        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (!isInputActive) return;

            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();

                if (isDrag || isPointerOverGameObject) return;

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.z));
                mouseWorldPosition = worldPos;
                // Vector3 origin = Camera.main.transform.position;
                // Vector3 direction = -(worldPos - origin).normalized;

                // if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
                // {
                //     if (hit.collider.TryGetComponent<Assets.Scripts.ControllUnit.ISelectableUnit>(out Assets.Scripts.ControllUnit.ISelectableUnit selectable))
                //     {
                //         selectableController.UnitFocusedPoint(selectable);
                //     }
                //     else
                //         selectableController.UnitFocusedPoint(null);
                // }
                // else
                // {
                //     selectableController.UnitFocusedPoint(null);
                // }
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
            OnDirectionChanged(sumOfDirection.normalized);
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
            if (context.started)
            {
                OnControllMenu?.Invoke();
            }
        }

        public string GetActionMapName() => actionMapName;
        public void ActionMapActivated() => isInputActive = true;
        public void ActionMapDeactivated() => isInputActive = false;
    }
}

