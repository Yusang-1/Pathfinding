using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class PlayerControllInput : MonoBehaviour, IActionMapInputer
    {
        public event Action<Vector2> OnDirectionChanged;

        private SelectableController selectableController;
        
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

        public void Initialize(SelectableController selectableController)
        {
            this.selectableController = selectableController;
        }

        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive) return;

            if (context.canceled)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.z));
                Vector3 origin = Camera.main.transform.position;
                Vector3 direction = -(worldPos - origin).normalized;
                Debug.DrawRay(origin, direction * 10, Color.green, 1.2f);
                if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
                {
                    if (hit.collider.TryGetComponent<ISelectableUnit>(out ISelectableUnit selectable))
                    {
                        selectableController.Selected(selectable);
                        // OnSelectedCallback(selectable.GetActionMapString());
                    }
                    else
                        selectableController.Selected(null);
                }
                else
                    selectableController.Selected(null);
            }
        }

        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive) return;
            
            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();
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

        public string GetActionMapName() => actionMapName;
        public void ActionMapActivated() => isInputActive = true;
        public void ActionMapDeactivated() => isInputActive = false;
    }
}
