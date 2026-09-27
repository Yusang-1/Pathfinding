using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

namespace Assets.Scripts.CreateMap
{
    public class PlayerControllInput : MonoBehaviour, IActionMapInputer
    {
        public event Action OnControllMenu;
        public event Action<Vector2> OnMoveScreen;

        private NodeList nodeList;
        private SelectableController selectableController;
        private MoveScreenJudger moveScreenJudger;

        [SerializeField] private ActionMaps actionMap;

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

        public void Initialize(SelectableController selectableController, NodeList nodeList, MoveScreenJudger moveScreenJudger)
        {
            this.selectableController = selectableController;
            this.nodeList = nodeList;
            this.moveScreenJudger = moveScreenJudger;
        }

        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject) return;

            if (context.canceled)
            {
                Vector2 origin = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));

                var index = nodeList.GetNodeIndex(origin);
                if (nodeList.TryGetNode(index, out Node node))
                {
                    selectableController.Selected(node);
                }
                else
                {
                    selectableController.Selected(null);
                }
            }
        }
                
        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();
                
                var viewPortPosition = Camera.main.ScreenToViewportPoint(mousePosition);
                if(moveScreenJudger.TryGetScreenMoveVelocity(viewPortPosition, out Vector2 velocity))
                {
                    OnMoveScreen?.Invoke(velocity);
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

        public ActionMaps GetActionMap() => actionMap;

        public void ActionMapActivated() => isInputActive = true;

        public void ActionMapDeactivated() => isInputActive = false;
    }
}
