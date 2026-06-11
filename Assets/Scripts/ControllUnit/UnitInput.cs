using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class UnitInput : MonoBehaviour
    {
        public event Action<Vector2> OnDirectionChanged;

        private Vector2 sumOfDirection;
        private readonly Dictionary<int, Vector2> directionDict = new();

        private Vector2 mousePosition;
        private bool isPointerOverGameObject;
        private SelectableController selectableController;
        
        private bool isInputActive;
        
        private void Update()
        {
            if(!isInputActive) return;
            
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
            if (isPointerOverGameObject) return;

            if (context.canceled)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.z)); // -Camera.main.transform.position.z
                Vector3 origin = Camera.main.transform.position;
                Vector3 direction = -(mousePos - origin).normalized;

                if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
                {
                    if (hit.collider.TryGetComponent<ISelectable>(out ISelectable selectable))
                    {
                        selectableController.Selected(selectable);                        
                    }
                    else
                        selectableController.Selected(null);
                }
                else
                    selectableController.Selected(null);
            }
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            Debug.Log("Right Click");
        }

        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
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
    }
}
