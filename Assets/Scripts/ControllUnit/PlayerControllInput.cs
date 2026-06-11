using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.ControllUnit
{
    public class PlayerControllInput : MonoBehaviour
    {
        public event Action<string> OnSelectedCallback;
        
        private Vector2 mousePosition;
        private bool isPointerOverGameObject;
        private readonly SelectableController selectableController = new();

        private void Update()
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

        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject) return;

            if (context.canceled)
            {
                Vector2 origin = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));

                RaycastHit2D hit2D = Physics2D.Raycast(origin, Vector3.forward, Mathf.Infinity);
                if (hit2D)
                {
                    if (hit2D.collider.TryGetComponent<ISelectable>(out ISelectable selectable))
                    {
                        // node.Selected();
                        selectableController.Selected(selectable);
                        OnSelectedCallback(selectable.ToString());
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
            
        }

        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();
            }
        }
    }
}
